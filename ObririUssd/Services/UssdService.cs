using Microsoft.EntityFrameworkCore;
using ObririUssd.Data;
using ObririUssd.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ObririUssd.Services
{
    public class UssdService : IUssdService
    {
        static ConcurrentDictionary<string, UserState> _previousState;
        static ConcurrentDictionary<string, UserState> PreviousState = _previousState ?? new ConcurrentDictionary<string, UserState>();
        Dictionary<string, string> Options = new Dictionary<string, string>
        {
            { "1", "Mon.-PIONEER" },{ "2", "Tue.-VAG EAST" },{ "3", "Wed.-VAG WEST" },{ "4", "Thur.-AFRICAN LOTTO" },{ "5", "Fri.-OBIRI SPECIAL" },{ "6", "Sat.-OLD SOLDIER" },{ "7", "SUN.-SPECIAL" }
        };
        Dictionary<string, string> DaysOfTheWeek = new Dictionary<string, string>
        {
            { "Monday", "1" },{ "Tuesday", "2" },{ "Wednesday", "3" },{ "Thursday", "4" },{ "Friday", "5" },{ "Saturday", "6" },{ "Sunday", "7" }
        };
        Dictionary<string, string> OptionsOfTheWeek = new Dictionary<string, string>
        {
            { "1", "1. Mon.-PIONEER\n2. Mon.-SPECIAL" },{ "2", "Tue.-VAG EAST" },{ "3", "Wed.-VAG WEST" },{ "4", "Thur.-AFRICAN LOTTO" },{ "5", "Fri.-OBIRI SPECIAL" },{ "6", "1. Sat.-OLD SOLDIER\n2. Sat.-NATIONAL" },{ "7", "SUNDAY\n1. SUN.-SPECIAL" }
        };
        Dictionary<string, Dictionary<string, string>> OptionsOfTheDay = new Dictionary<string, Dictionary<string, string>>
        {
            { "1", new Dictionary<string, string>{{ "1", "Mon.-PIONEER" },{ "2", "Mon.-SPECIAL" }}},
            { "2", new Dictionary<string, string>{{ "1", "Tue.-VAG EAST" },{ "2", "Tue.-LUCKY" }}},
            { "3", new Dictionary<string, string>{{ "1", "Wed.-VAG WEST" },{ "2", "Wed.-MIDWEEK" }}},
            { "4", new Dictionary<string, string>{{ "1", "Thur.-AFRICAN LOTTO" },{ "2", "Thur.-FORTUNE" }}},
            { "5", new Dictionary<string, string>{{ "1", "Fri.-OBIRI SPECIAL" },{ "2", "Fri.-BONANZA" }}},
            { "6", new Dictionary<string, string>{{ "1", "Sat.-OLD SOLDIER" },{ "2", "Sat.-NATIONAL" }}},
            { "7", new Dictionary<string, string>{{ "1", "Sun.-SPECIAL" }}}
        };
        private UserState state = null;
        private string userid = "WEB_MATE";
        private UssdDataContext _context;

        public UssdService(UssdDataContext context)
        {
            _context = context;
        }
        public async Task<UssdResponse> ProcessRequest(UssdRequest request)
        {
            var duration = await _context.UssdLock.FirstOrDefaultAsync();
            if (duration.Disabled)
            {
                PreviousState.TryRemove(request.MSISDN, out UserState _);
                return UssdResponse.CreateResponse(userid, request.MSISDN, "Sorry Draw is closed", false);
            }

            if (duration is not null && duration.DrawHasEnded())
            {
                PreviousState.TryRemove(request.MSISDN, out UserState _);
                return UssdResponse.CreateResponse(userid, request.MSISDN, "Sorry Draw Has Ended", false);
            }

            IncreaseState(request);

            if (state?.CurrentState?.Length == 1 && string.IsNullOrWhiteSpace(request.USERDATA))
            {
                if (PreviousState.TryGetValue(request.MSISDN, out state))
                {
                    PreviousState.TryRemove(request.MSISDN, out UserState tt);
                    var _state = tt with { CurrentState = "" };
                    PreviousState.TryAdd(request.MSISDN, _state);
                    PreviousState.TryGetValue(request.MSISDN, out state);
                }
                Options.TryGetValue(DaysOfTheWeek[DateTime.Now.DayOfWeek.ToString()], out string option);
                return ProcessMenu(request, $"{option}\n2.Direct-2\n3.Direct-3\n4.Direct-4\n5.Direct-5\n6.Perm 2\n7.Perm-3\n");
            }

            if (state?.CurrentState?.Length == 2)
            {
                var day = DaysOfTheWeek[DateTime.Now.DayOfWeek.ToString()];
                Options.TryGetValue(day, out string option);
                if (!request.ValidateInputFormat())
                {
                    DecreaseState(request);
                    return UssdResponse.CreateResponse(userid, request.MSISDN, "Input value is not in the rigth format", true);
                }

                if (!request.ValidateInputRange(7, 2))
                {
                    DecreaseState(request);
                    return UssdResponse.CreateResponse(userid, request.MSISDN, $"Enter value between {2} - {7}", true);
                }
                var key = request.USERDATA + state.CurrentState;
                var message = GetSubmenus(key, option, request.USERDATA);
                return ProcessSubMenu(request, message);
            }
            if (state?.CurrentState?.Length == 3 && string.IsNullOrWhiteSpace(request.USERDATA))
            {
                PreviousState.TryRemove(request.MSISDN, out UserState userState);
                var state = userState with { CurrentState = userState.CurrentState[0..^1], PreviousData = userState.PreviousData };
                PreviousState.TryAdd(request.MSISDN, state);
                return UssdResponse.CreateResponse(userid, request.MSISDN, $"Please Enter {state?.PreviousData} distinct number(s) from (1-90). \n Separate each number with a space ", true);
            }
            else if (state?.CurrentState?.Length == 3)
            {
                if (int.TryParse(state?.PreviousData, out int previousData))
                {

                    if (previousData.Equals(request.USERDATA.Split(" ").Length) && request.ValidateInputFormats() && request.ValidateInputRanges(90, 1) && !request.HasDuplicate())
                    {
                        PreviousState.TryRemove(request.MSISDN, out UserState t);
                        var state = t with { SelectedValues = request.USERDATA };
                        PreviousState.TryAdd(request.MSISDN, state);
                        return UssdResponse.CreateResponse(userid, request.MSISDN, "Enter amount", true);
                    }
                    PreviousState.TryRemove(request.MSISDN, out UserState tt);
                    var _state = tt with { CurrentState = tt.CurrentState[0..^1], PreviousData = tt.PreviousData, SelectedValues = request.USERDATA };
                    PreviousState.TryAdd(request.MSISDN, _state);
                    return UssdResponse.CreateResponse(userid, request.MSISDN, $"Please Enter {previousData} distinct number(s) from (1-90). \n Separate each number with a space ", true);
                }
            }
            else if (state?.CurrentState?.Length == 4)
            {
                if (!int.TryParse(request.USERDATA, out int r))
                {
                    DecreaseState(request);
                    return new UssdResponse
                    {
                        USERID = userid,
                        MSISDN = request.MSISDN,
                        MSG = "Input value is not in the rigth format",
                        MSGTYPE = true
                    };
                }
                if (float.Parse(request.USERDATA) < 10)
                {
                    PreviousState.TryRemove(request.MSISDN, out UserState userState);
                    var state = userState with { CurrentState = userState.CurrentState[0..^1], PreviousData = userState.PreviousData };
                    PreviousState.TryAdd(request.MSISDN, state);
                    return UssdResponse.CreateResponse(userid, request.NETWORK, "Transaction amount below GHS 10.00 are not allowed", true);
                }

                var response = await request.ProcessPayment();

                //var result = JsonSerializer.Deserialize<PaymentResponse>(response.Content);
                if (true) //(response.Content.Contains("approved"))
                {
                    var mainMenuItem = OptionsOfTheDay[DaysOfTheWeek[DateTime.Now.DayOfWeek.ToString()]];
                    mainMenuItem.TryGetValue(state.UserOption, out string optionName);
                    var m = GetFinalStates(state.PreviousData, optionName, request.USERDATA);

                    return await ProcessFinalState(request, m.Message, m.Option, state.SelectedValues);
                }
                PreviousState.TryRemove(request.MSISDN, out UserState _);
                return UssdResponse.CreateResponse(userid, request.MSISDN, "Error", false);
            }
            else if (!string.IsNullOrWhiteSpace(request?.USERDATA) && !string.IsNullOrWhiteSpace(state?.CurrentState))
            {

                OptionsOfTheDay[DaysOfTheWeek[DateTime.Now.DayOfWeek.ToString()]].TryGetValue(request.USERDATA,out string optionsOfTheDay);
                if(optionsOfTheDay is null)
                {
                    DecreaseState(request);
                    var _opt = OptionsOfTheWeek[DaysOfTheWeek[DateTime.Now.DayOfWeek.ToString()]];
                    return ProcessMenu(request, $"{_opt}");
                }
                return ProcessMenu(request, $"{optionsOfTheDay}\n2.Direct-2\n3.Direct-3\n4.Direct-4\n5.Direct-5\n6.Perm 2\n7.Perm-3\n");
            }
            var _option = OptionsOfTheWeek[DaysOfTheWeek[DateTime.Now.DayOfWeek.ToString()]];
            return ProcessMenu(request, $"{_option}");
            //Options.TryGetValue(DaysOfTheWeek[DateTime.Now.DayOfWeek.ToString()], out string _option);
            // return ProcessMenu(request, $"{_option}\n2.Direct-2\n3.Direct-3\n4.Direct-4\n5.Direct-5\n6.Perm 2\n7.Perm-3\n");
        }

        private void DecreaseState(UssdRequest request)
        {
            var _state = new UserState { CurrentState = state.CurrentState.Length == 1 ? "" : state.CurrentState.Substring(0, 2), PreviousData = request.USERDATA };
            PreviousState.TryRemove(request.MSISDN, out UserState tt);
            PreviousState.TryAdd(request.MSISDN, _state);
        }

        private void IncreaseState(UssdRequest request)
        {
            if (PreviousState.TryGetValue(request.MSISDN, out state))
            {
                PreviousState.TryRemove(request.MSISDN, out UserState tt);
                var _state = tt with { CurrentState = tt.CurrentState + "1" };
                PreviousState.TryAdd(request.MSISDN, _state);
                PreviousState.TryGetValue(request.MSISDN, out state);
            }
        }

        private string GetSubmenus(string key, string optionValve, string option)
        {
            switch (key)
            {
                //Userdata+CurrentState
                case "11":
                case "12":
                case "13":
                case "14":
                case "15":
                case "16":
                case "17":
                    return $"{option}\n1.Direct-1\nEnter 1 number(s) from (1-90)";
                //option 6
                case "61":
                case "62":
                case "63":
                case "64":
                case "65":
                case "66":
                case "67":
                    //case "7171":
                    return $"{option}\n6.Perm - 2 \nEnter 3 distinct number(s) from (1-90)";

                //option 7
                case "71":
                case "72":
                case "73":
                case "74":
                case "75":
                case "76":
                case "77":
                    //case "7171":
                    return $"{option}\n7.Perm - 3 \nEnter 4 distinct number(s) from (1-90)";



            }
            return $"{optionValve}:\n{option}.Direct-{option}\nEnter {option} distinct number(s) from (1-90).\n Separate each number with a space ";
        }

        private MessageType GetFinalStates(string previousValue, string optionName, string amount)
        {
            switch (previousValue)
            {
                //previousData+Userdata+CurrentState
                case "6":
                    return new MessageType { Message = $"Your ticket: {optionName}:Perm-2,  GHS {amount} is registered for Perm - 2. Id", Option = $"{optionName}:Perm - 2" };
                case "7":
                    return new MessageType { Message = $"Your ticket: {optionName}:Perm-3,  GHS {amount} is registered for Perm - 3. Id", Option = $"{optionName}:Perm - 3" };
            }
            return new MessageType { Message = $"Your ticket: {optionName} Direct-{previousValue},  GHS {amount} is registered for Direct - {previousValue}. Id", Option = $"{optionName}:Direct - {previousValue}" };
        }

        private async Task<UssdResponse> ProcessFinalState(UssdRequest request, string message, string option,string optionValue)
        {

            PreviousState.TryRemove(request.MSISDN, out UserState tt);
            var transaction = new UssdTransaction
            {
                Amount = int.Parse(request.USERDATA),
                OptionName = option,
                OptionValue = optionValue,
                PhoneNumber = request.MSISDN
            };
            _context.Add(transaction);
            await _context.SaveChangesAsync();
            var savedTransaction = await _context.Trans.FindAsync(transaction.Id);
            savedTransaction.Message = $"{message}:{transaction.Id} {DateTime.Now}";
            savedTransaction.Status = true;
            await _context.SaveChangesAsync();

            await new MessageService().SendSms(request.MSISDN, $"{message}:{transaction.Id} {DateTime.Now}");
            PreviousState.TryRemove(request.MSISDN, out UserState _);

            return new UssdResponse
            {
                USERID = userid,
                MSISDN = request.MSISDN,
                MSG = "Success",
                MSGTYPE = false
            };
        }

        private UssdResponse ProcessSubMenu(UssdRequest request, string message)
        {
            PreviousState.TryRemove(request.MSISDN, out UserState tt);
            var _state = tt with { CurrentState = state.CurrentState, PreviousData = request.USERDATA };
            PreviousState.TryAdd(request.MSISDN, _state);
            return UssdResponse.CreateResponse(userid, request.MSISDN, message, true);
        }

        private UssdResponse ProcessMenu(UssdRequest request, string message)
        {
            UserState _state;
            PreviousState.TryRemove(request.MSISDN, out UserState s);
            if(s is null)
            {
                _state = new UserState { UserOption = request.USERDATA, CurrentState = "", PreviousData = request.USERDATA };
            }
            else
            {
                _state = s with { UserOption = request.USERDATA, PreviousData = request.USERDATA };
            } 
            PreviousState.TryAdd(request.MSISDN, _state);
            return UssdResponse.CreateResponse(userid, request.MSISDN, message, true);
        }
    }
}
