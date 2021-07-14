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
            { "1", "Mon.-PIONEER" },{ "2", "Tue.-VAG EAST" },{ "3", "Wed.-VAG EAST" },{ "4", "Thur.-AFRICAN LOTTO" },{ "5", "Fri.-OBIRI FRIDAY" },{ "6", "Sat.-OLD SOLDIER" },{ "7", "SUN.-SPECIAL" }
        };
        Dictionary<string, string> DaysOfTheWeek = new Dictionary<string, string>
        {
            { "Monday", "1" },{ "Tuesday", "2" },{ "Wednesday", "3" },{ "Thursday", "4" },{ "Friday", "5" },{ "Saturday", "6" },{ "Sunday", "7" }
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
            if (duration is not null && duration.DrawHasEnded())
                return UssdResponse.CreateResponse(userid, request.MSISDN, "Sorry Draw Has Ended", false);
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
                return ProcessMenu(request, $"{option}\n1.Direct-1\n2.Direct-2\n3.Direct-3\n4.Direct-4\n5.Direct-5\n6.Perm 2\n7.Perm-3\n");
            }
            
            if (state?.CurrentState?.Length == 2)
            {
                if (int.TryParse(state?.PreviousData, out int previousData))
                {
                    //if (previousData.Equals(1))
                    //{
                    //    if (previousData.Equals(request.USERDATA.Length) && request.ValidateInputFormat() && request.ValidateInputRange(91,1))
                    //       return UssdResponse.CreateResponse(userid, request.MSISDN, "Enter amount", true);
                    //    PreviousState.TryRemove(request.MSISDN, out UserState userState);
                    //    var state = userState with { CurrentState = userState.CurrentState.Substring(0, userState.CurrentState.Length - 1) };
                    //    PreviousState.TryAdd(request.MSISDN, state);
                    //    return UssdResponse.CreateResponse(userid, request.MSISDN, $"Please Enter {previousData} number from (1-90).", true);
                    //}
                    if (previousData.Equals(request.USERDATA.Split(" ").Length) && request.ValidateInputFormats() && request.ValidateInputRanges(90, 1))
                    {
                        return UssdResponse.CreateResponse(userid, request.MSISDN, "Enter amount", true);
                    }
                    PreviousState.TryRemove(request.MSISDN, out UserState tt);
                    var _state = tt with { CurrentState = tt.CurrentState[0..^1], PreviousData = tt.PreviousData };
                    PreviousState.TryAdd(request.MSISDN, _state);
                    return UssdResponse.CreateResponse(userid, request.MSISDN, $"Please Enter {previousData} number(s) from (1-90). \n Separate each number with a space ", true);
                }
            }
            if (state?.CurrentState?.Length == 3 && string.IsNullOrWhiteSpace(request.USERDATA))
            {
                PreviousState.TryRemove(request.MSISDN, out UserState userState);
                var state = userState with { CurrentState = userState.CurrentState[0..^1], PreviousData = userState.PreviousData };
                PreviousState.TryAdd(request.MSISDN, state);
                return UssdResponse.CreateResponse(userid, request.MSISDN, $"Please Enter {state?.PreviousData} number(s) from (1-90). \n Separate each number with a space ", true);
            }
            else if (state?.CurrentState?.Length == 3)
            {
                if (!int.TryParse(request.USERDATA, out int result))
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
                // HttpResponseMessage response = await request.ProcessPayment();

                //if (response.IsSuccessStatusCode)
                //{
                //    string jsonResponse = await response.Content.ReadAsStringAsync();
                //    var result = JsonSerializer.Deserialize<PaymentResponse>(jsonResponse);
                //    if (result.status == "approved")
                //    {
                var mainMenuItem = DaysOfTheWeek[DateTime.Now.DayOfWeek.ToString()];
                         Options.TryGetValue(mainMenuItem, out string optionName);
                         var m = GetFinalStates(state.PreviousData, optionName, request.USERDATA);
                         //PreviousState.TryRemove(request.MSISDN, out UserState tt);
                         return await ProcessFinalState(request, m.Message, m.Option, request.USERDATA);
                     //}
                    //PreviousState.TryRemove(request.MSISDN, out UserState _);
                    //return UssdResponse.CreateResponse(userid, request.MSISDN, "Error", false);
                //}
                // else
                // {
                //    string jsonResponse = await response.Content.ReadAsStringAsync();
                //    var result = JsonSerializer.Deserialize<PaymentResponse>(jsonResponse);
                //    PreviousState.TryRemove(request.MSISDN, out UserState tt);
                //    return UssdResponse.CreateResponse(userid, request.MSISDN, "Error", false);
                // }
            }

            else if (!string.IsNullOrWhiteSpace(request?.USERDATA) && !string.IsNullOrWhiteSpace(state?.CurrentState))
            {
                var day = DaysOfTheWeek[DateTime.Now.DayOfWeek.ToString()];
                Options.TryGetValue(day, out string option);
                if (!request.ValidateInputFormat())
                {
                    DecreaseState(request);
                    return UssdResponse.CreateResponse(userid, request.MSISDN, "Input value is not in the rigth format", true);
                }

                if (!request.ValidateInputRange(7, 1))
                {
                    DecreaseState(request);
                    return UssdResponse.CreateResponse(userid, request.MSISDN, "Enter value between 1 - 7", true);
                }
                var key = request.USERDATA + state.CurrentState;
                var message = GetSubmenus(key, option, request.USERDATA);
                return ProcessSubMenu(request, message);
            }

            Options.TryGetValue(DaysOfTheWeek[DateTime.Now.DayOfWeek.ToString()], out string _option);
            return ProcessMenu(request, $"{_option}\n1.Direct-1\n2.Direct-2\n3.Direct-3\n4.Direct-4\n5.Direct-5\n6.Perm 2\n7.Perm-3\n");
        }

        private void DecreaseState(UssdRequest request)
        {
            var _state = new UserState { CurrentState = state.CurrentState.Substring(0, 2), PreviousData = request.USERDATA };
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
                    return $"{option}\n6.Perm - 2 \nEnter 3 number(s) from (1-90)";

                //option 7
                case "71":
                case "72":
                case "73":
                case "74":
                case "75":
                case "76":
                case "77":
                    //case "7171":
                    return $"{option}\n7.Perm - 3 \nEnter 4 number(s) from (1-90)";



            }
            return $"{optionValve}:\n{option}.Direct-{option}\nEnter {option} number(s) from (1-90).\n Separate each number with a space ";
        }

        private MessageType GetFinalStates(string previousValue, string optionName, string option)
        {
            switch (previousValue)
            {
                //previousData+Userdata+CurrentState
                case "6":
                    return new MessageType { Message = $"Your ticket: {optionName}:Perm-2,  1GHS is registered for Perm - 2. Id", Option = $"{optionName}:Perm - 2" };
                case "7":
                    return new MessageType { Message = $"Your ticket: {optionName}:Perm-3,  1GHS is registered for Perm - 3. Id", Option = $"{optionName}:Perm - 3" };
            }
            return new MessageType { Message = $"Your ticket: {optionName} Direct-{previousValue},  1GHS is registered for Direct - {option}. Id", Option = $"{optionName}:Direct - {previousValue}" };
        }

        private async Task<UssdResponse> ProcessFinalState(UssdRequest request, string message, string option,string amount)
        {

            PreviousState.TryRemove(request.MSISDN, out UserState tt);
            var transaction = new UssdTransaction
            {
                Amount = int.Parse(amount),
                OptionName = option,
                OptionValue = request.USERDATA,
                PhoneNumber = request.MSISDN
            };
            _context.Add(transaction);
            await _context.SaveChangesAsync();

            await new MessageService().SendSms(request.MSISDN, $"{message}:{transaction.Id}");

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
            var _state = new UserState { CurrentState = state.CurrentState, PreviousData = request.USERDATA };
            PreviousState.TryRemove(request.MSISDN, out UserState tt);
            PreviousState.TryAdd(request.MSISDN, _state);
            return UssdResponse.CreateResponse(userid, request.MSISDN, message, true);
        }

        private UssdResponse ProcessMenu(UssdRequest request, string message)
        {
            var _state = new UserState { CurrentState = "", PreviousData = request.USERDATA };
            PreviousState.TryAdd(request.MSISDN, _state);
            return UssdResponse.CreateResponse(userid, request.MSISDN, message, true);
        }
    }
}
