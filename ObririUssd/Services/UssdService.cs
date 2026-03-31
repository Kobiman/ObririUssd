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
using static ObririUssd.UssdSessionManager;

namespace ObririUssd.Services
{
    public class UssdService : IUssdService
    {
        
        Dictionary<string, string> DaysOfTheWeek = new Dictionary<string, string>
        {
            { "Monday", "1" },{ "Tuesday", "2" },{ "Wednesday", "3" },{ "Thursday", "4" },{ "Friday", "5" },{ "Saturday", "6" },{ "Sunday", "7" }
        };
        Dictionary<string, string> OptionsOfTheWeek = new Dictionary<string, string>
        {
            { "1", "Welcome to VAG-Obiri Lotteries\n1. PIONEER\n2. MONDAY SPECIAL" },{ "2", "Welcome to VAG-Obiri Lotteries\n1. VAG EAST\n2. LUCKY TUESDAY" },{ "3", "Welcome to VAG-Obiri Lotteries\n1. VAG WEST\n 2. MID-WEEK" },{ "4", "Welcome to VAG-Obiri Lotteries\n1. AFRICAN LOTTO\n2. FORTUNE THURSDAY" },{ "5", "Welcome to VAG-Obiri Lotteries\n1. OBIRI SPECIAL\n2. FRIDAY BONANZA" },{ "6", "Welcome to VAG-Obiri Lotteries\n1. OLD SOLDIER\n2. NATIONAL" },{ "7", "1. SUNDAY SPECIAL" }
        };
        Dictionary<string, Dictionary<string, string>> OptionsOfTheDay = new Dictionary<string, Dictionary<string, string>>
        {
            { "1", new Dictionary<string, string>{{ "1", "PIONEER" },{ "2", "MONDAY SPECIAL" }}},
            { "2", new Dictionary<string, string>{{ "1", "VAG EAST" },{ "2", "LUCKY TUESDAY" }}},
            { "3", new Dictionary<string, string>{{ "1", "VAG WEST" },{ "2", "MID-WEEK" }}},
            { "4", new Dictionary<string, string>{{ "1", "AFRICAN LOTTO" },{ "2", "FORTUNE THURSDAY" }}},
            { "5", new Dictionary<string, string>{{ "1", "OBIRI SPECIAL" },{ "2", "FRIDAY BONANZA" }}},
            { "6", new Dictionary<string, string>{{ "1", "OLD SOLDIER" },{ "2", "NATIONAL" }}},
            { "7", new Dictionary<string, string>{{ "1", "SUNDAY SPECIAL" }}}
        };

        private UserState state = null;
        private string userid = "HO4AHOI73X_BR@6d";
        private UssdDataContext _context;
        private readonly IPaymentChannel _channel;

        public UssdService(UssdDataContext context, IPaymentChannel channel)
        {
            _context = context;
            _channel = channel;
        }
        public async Task<UssdResponse> ProcessRequest(UssdRequest request, System.Threading.CancellationToken token)
        {
            try
            {
                if(PreviousState.TryGetValue(request.SessionID, out UserState s))
                {
                    var duration = await _context.UssdLock.FirstOrDefaultAsync(x=>x.GameType == s.GameType, cancellationToken: token);
                    if(duration != null)
                    {
                        if (duration.Disabled)
                        {
                            PreviousState.TryRemove(request.SessionID, out _);
                            return UssdResponse.CreateResponse(userid, request.Msisdn, $"Sorry Draw is closed for {s.GameType}", false);
                        }

                        if (duration.DrawHasEnded())
                        {
                            PreviousState.TryRemove(request.SessionID, out _);
                            return UssdResponse.CreateResponse(userid, request.Msisdn, $"Sorry Draw Has Ended for {s.GameType}", false);
                        }
                    }
                }
                

                IncreaseState(request);


                if (state?.CurrentState?.Length == 1 && string.IsNullOrWhiteSpace(request.UserData))
                {
                    token.ThrowIfCancellationRequested();
                    if (PreviousState.TryGetValue(request.SessionID, out state))
                    {
                        PreviousState.TryRemove(request.SessionID, out UserState tt);
                        var _state = tt with { CurrentState = "" };
                        PreviousState.TryAdd(request.SessionID, _state);
                        PreviousState.TryGetValue(request.SessionID, out state);
                    }
                    OptionsOfTheWeek.TryGetValue(DaysOfTheWeek[DateTime.Now.DayOfWeek.ToString()], out string option);
                    return ProcessMenu(request, $"{option}");
                }

                if (state?.CurrentState?.Length == 2 )
                {
                    token.ThrowIfCancellationRequested();
                    OptionsOfTheDay[DaysOfTheWeek[DateTime.Now.DayOfWeek.ToString()]].TryGetValue(state.UserOption, out string optionsOfTheDay);

                    if (!request.ValidateInputFormat())
                    {
                        DecreaseState(request);
                        return UssdResponse.CreateResponse(userid, request.Msisdn, "Input value is not in the rigth format", true);
                    }

                    if (!request.ValidateInputRange(7, 2))
                    {
                        DecreaseState(request);
                        return UssdResponse.CreateResponse(userid, request.Msisdn, $"Enter value between {2} - {7}", true);
                    }
                    //var key = request.USERDATA + state.CurrentState;
                    var message = GetSubmenus(optionsOfTheDay, request.UserData);
                    return ProcessSubMenu(request, message);
                }
                if (state?.CurrentState?.Length == 3 && string.IsNullOrWhiteSpace(request.UserData))
                {
                    token.ThrowIfCancellationRequested();
                    PreviousState.TryRemove(request.SessionID, out UserState userState);
                    var state = userState with { CurrentState = userState.CurrentState[0..^1], PreviousData = userState.PreviousData };
                    PreviousState.TryAdd(request.SessionID, state);
                    return UssdResponse.CreateResponse(userid, request.Msisdn, $"Please Enter {state?.PreviousData} distinct number(s) from (1-90). \n Separate each number with a space ", true);
                }
                else if (state?.CurrentState?.Length == 3)
                {
                    token.ThrowIfCancellationRequested();
                    if (int.TryParse(state?.PreviousData, out int previousData))
                    {
                        if (previousData.Equals(7))//Perm 3
                        {
                            if (request.UserData.Split(" ").Length >= 4 && request.ValidateInputFormats() && request.ValidateInputRanges(90, 1) && !request.HasDuplicate())
                            {
                                PreviousState.TryRemove(request.SessionID, out UserState t);
                                var state = t with { SelectedValues = request.UserData };
                                PreviousState.TryAdd(request.SessionID, state);
                                return UssdResponse.CreateResponse(userid, request.Msisdn, "Enter amount", true);
                            }
                        }
                        if (previousData.Equals(6))//Perm 2
                        {
                            if (request.UserData.Split(" ").Length >= 3 && request.ValidateInputFormats() && request.ValidateInputRanges(90, 1) && !request.HasDuplicate())
                            {
                                PreviousState.TryRemove(request.SessionID, out UserState t);
                                var state = t with { SelectedValues = request.UserData };
                                PreviousState.TryAdd(request.SessionID, state);
                                return UssdResponse.CreateResponse(userid, request.Msisdn, "Enter amount", true);
                            }
                        }
                        else
                        {
                            if (previousData.Equals(request.UserData.Split(" ").Length) && request.ValidateInputFormats() && request.ValidateInputRanges(90, 1) && !request.HasDuplicate())
                            {
                                PreviousState.TryRemove(request.SessionID, out UserState t);
                                var state = t with { SelectedValues = request.UserData };
                                PreviousState.TryAdd(request.SessionID, state);
                                return UssdResponse.CreateResponse(userid, request.Msisdn, "Enter amount", true);
                            }
                        }

                        PreviousState.TryRemove(request.SessionID, out UserState tt);
                        var _state = tt with { CurrentState = tt.CurrentState[0..^1], PreviousData = tt.PreviousData, SelectedValues = request.UserData };
                        PreviousState.TryAdd(request.SessionID, _state);
                        return UssdResponse.CreateResponse(userid, request.Msisdn, $"Please Enter {previousData} distinct number(s) from (1-90). \n Separate each number with a space ", true);
                    }
                }
                else if (state?.CurrentState?.Length == 4)
                {
                    if (!int.TryParse(request.UserData, out int r))
                    {
                        DecreaseState(request);
                        return new UssdResponse
                        {
                            UserID = userid,
                            Msisdn = request.Msisdn,
                            Message = "Input value is not in the rigth format",
                            ContinueSession = true,
                           // MSGTYPE = true
                        };
                    }
                    //if (float.Parse(request.USERDATA) < 10)
                    //{
                    //    PreviousState.TryRemove(request.MSISDN, out UserState userState);
                    //    var state = userState with { CurrentState = userState.CurrentState[0..^1], PreviousData = userState.PreviousData };
                    //    PreviousState.TryAdd(request.MSISDN, state);
                    //    return UssdResponse.CreateResponse(userid, request.NETWORK, "Transaction amount below GHS 10.00 are not allowed", true);
                    //}

                    request.UserData = int.Parse(state.PreviousData) > 5 ? (int.Parse(request.UserData) * request.GetNumberOfLines(state.PreviousData, state.SelectedValues.Split(" ").Length)).ToString() : request.UserData;
                    //var response = await request.ProcessPayment();
                    //if(true) //(response.Content.Contains("approved"))
                    //{
                    //    var mainMenuItem = OptionsOfTheDay[DaysOfTheWeek[DateTime.Now.DayOfWeek.ToString()]];
                    //    mainMenuItem.TryGetValue(state.UserOption, out string optionName);
                    //    var m = GetFinalStates(state.PreviousData, optionName, request.USERDATA);

                    //    return await ProcessFinalState(request, m.Message, m.Option, state.SelectedValues);
                    //}
                    await _channel.WriteAsync(new PaymentChannelMessage(request,state));
                    PreviousState.TryRemove(request.SessionID, out _);
                    //var result = JsonSerializer.Deserialize<PaymentResponse>(response.Content);
                    return UssdResponse.CreateResponse(userid, request.Msisdn, $"Processing transaction, please wait... go to approvals if you do not receive prompt.", false);
                }
                else if (!string.IsNullOrWhiteSpace(request?.UserData) && !string.IsNullOrWhiteSpace(state?.CurrentState))
                {
                    var dayOfWeek = DateTime.Now.DayOfWeek.ToString();
                    OptionsOfTheDay[DaysOfTheWeek[dayOfWeek]].TryGetValue(request.UserData, out string optionsOfTheDay);
                    if (optionsOfTheDay is null)
                    {
                        DecreaseState(request);
                        var _opt = OptionsOfTheWeek[DaysOfTheWeek[dayOfWeek]];
                        return ProcessMenu(request, $"{_opt}");
                    }
                    var gameType = request.GameTypes()[optionsOfTheDay];
                    return ProcessMenu(request, $"{optionsOfTheDay}\n2.Direct-2\n3.Direct-3\n4.Direct-4\n5.Direct-5\n6.Perm 2\n7.Perm-3\n", gameType);
                }
                var day = DateTime.Now.DayOfWeek.ToString();
                var _option = OptionsOfTheWeek[DaysOfTheWeek[day]];
                return ProcessMenu(request, $"{day}\n{_option}");
            }
            catch (OperationCanceledException)
            {
                PreviousState.TryRemove(request.SessionID, out UserState _);
                return UssdResponse.CreateResponse(userid, request.Msisdn, "", false);
            }
        }

        private void DecreaseState(UssdRequest request)
        {
            var currentState = "";
            switch (state.CurrentState.Length)
            {
                case 2:
                    currentState = state.CurrentState.Substring(0, 1);
                    break;
                case 3:
                    currentState = state.CurrentState.Substring(0, 2);
                    break;
            }
            var _state = state with { CurrentState = currentState, PreviousData = request.UserData };
            PreviousState.TryRemove(request.SessionID, out UserState tt);
            PreviousState.TryAdd(request.SessionID, _state);
        }

        private void IncreaseState(UssdRequest request)
        {
            if (PreviousState.TryGetValue(request.SessionID, out state))
            {
                PreviousState.TryRemove(request.SessionID, out UserState tt);
                var _state = tt with { CurrentState = tt.CurrentState + "1" };
                PreviousState.TryAdd(request.SessionID, _state);
                PreviousState.TryGetValue(request.SessionID, out state);
            }
        }

        private string GetSubmenus(string optionValve, string option)
        {
            switch (option)
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
                //case "61":
                //case "62":
                //case "63":
                //case "64":
                //case "65":
                //case "66":
                //case "67":
                case "6":
                    //case "7171":
                    return $"{option}\n6.Perm - 2 \nEnter mimimun of 3 distinct number(s) from (1-90)";

                //option 7
                //case "71":
                //case "72":
                //case "73":
                //case "74":
                //case "75":
                //case "76":
                //case "77":
                case "7":
                    //case "7171":
                    return $"{option}\n7.Perm - 3 \nEnter mimimun of 4 distinct number(s) from (1-90)";



            }
            return $"{optionValve}:\n{option}.Direct-{option}\nEnter {option} distinct number(s) from (1-90).\n Separate each number with a space ";
        }

        private UssdResponse ProcessSubMenu(UssdRequest request, string message)
        {
            PreviousState.TryRemove(request.SessionID, out UserState tt);
            var _state = tt with { CurrentState = state.CurrentState, PreviousData = request.UserData };
            PreviousState.TryAdd(request.SessionID, _state);
            return UssdResponse.CreateResponse(userid, request.Msisdn, message, true);
        }

        private UssdResponse ProcessMenu(UssdRequest request, string message, string gameType = "")
        {
            UserState _state;
            PreviousState.TryRemove(request.SessionID, out UserState s);
            if(s is null)
            {
                _state = new UserState { UserOption = request.UserData, CurrentState = "", PreviousData = request.UserData };
            }
            else
            {
                if (string.IsNullOrWhiteSpace(gameType))
                {
                    _state = s with { UserOption = request.UserData, PreviousData = request.UserData };
                }
                else
                {
                    _state = s with { UserOption = request.UserData, PreviousData = request.UserData, GameType = gameType };
                }
                
            } 
            PreviousState.TryAdd(request.SessionID, _state);
            return UssdResponse.CreateResponse(userid, request.Msisdn, message, true);
        }
    }
}
