using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ObririUssd.Data;
using ObririUssd.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ObririUssd.Controllers
{
    //[Route("api/[controller]")]
    [ApiController]
    public class UssdController : ControllerBase
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
        public UssdController(UssdDataContext context)
        {
            _context = context;
        }

        [HttpPost("index")]
        public async Task<IActionResult> Index([FromBody] UssdRequest request)
        {
            if(DateTime.Now.Hour >= 18) return Ok(new UssdResponse
            {
                USERID = userid,
                MSISDN = request.MSISDN,
                MSG = "Sorry no draw has ended",
                MSGTYPE = false
            });

            if (DateTime.Now.Hour < 6) return Ok(new UssdResponse
            {
                USERID = userid,
                MSISDN = request.MSISDN,
                MSG = "Sorry no draw has ended",
                MSGTYPE = false
            });

            if (PreviousState.TryGetValue(request.MSISDN, out state))
            {
                PreviousState.TryRemove(request.MSISDN, out UserState tt);
                var _state = tt with { CurrentState = tt.CurrentState+"1" };
                PreviousState.TryAdd(request.MSISDN, _state);
                PreviousState.TryGetValue(request.MSISDN, out state);
            }

            if((string.IsNullOrWhiteSpace(request?.USERDATA) || request?.USERDATA == "*920*79") && state is null)
            {
                var day = DaysOfTheWeek[DateTime.Now.DayOfWeek.ToString()];
                Options.TryGetValue(day, out string option);
                var message = $"{option}\n1.Direct-1\n2.Direct-2\n3.Direct-3\n4.Direct-4\n5.Direct-5\n6.Perm 2\n7.Perm-3\n";
                return ProcessMenu(request, message);
            }
            else if (state?.CurrentState?.Length >= 2)
            {
                var mainMenuItem = DaysOfTheWeek[DateTime.Now.DayOfWeek.ToString()];
                Options.TryGetValue(mainMenuItem, out string optionName);                
                var m = GetFinalStates(state.PreviousData, optionName, request.USERDATA);
                return await ProcessFinalState(request, m.Message, m.Option);
            }
            else if(!string.IsNullOrWhiteSpace(request?.USERDATA) && !string.IsNullOrWhiteSpace(state?.CurrentState))
            {
                //var mainMenuItem = state.CurrentState.AsSpan().Slice(0, 1).ToString();
                var day = DaysOfTheWeek[DateTime.Now.DayOfWeek.ToString()];
                Options.TryGetValue(day, out string option);
                if (!int.TryParse(request.USERDATA, out int result))
                {
                    var _state = new UserState { CurrentState = "", PreviousData = request.USERDATA };
                    PreviousState.TryRemove(request.MSISDN, out UserState tt);
                    PreviousState.TryAdd(request.MSISDN, _state);
                    return Ok(new UssdResponse
                    {
                        USERID = userid,
                        MSISDN = request.MSISDN,
                        MSG = "Input value is not in the rigth format",
                        MSGTYPE = true
                    });
                }

                if (result > 7 || result < 1)
                {
                    var _state = new UserState { CurrentState = "", PreviousData = request.USERDATA };
                    PreviousState.TryRemove(request.MSISDN, out UserState tt);
                    PreviousState.TryAdd(request.MSISDN, _state);
                    return Ok(new UssdResponse
                    {
                        USERID = userid,
                        MSISDN = request.MSISDN,
                        MSG = "Entere value between 1 - 7",
                        MSGTYPE = true
                    });
                }
                //previousData+Userdata+CurrentState
                var key = request.USERDATA+state.CurrentState;
                var message = GetSubmenus(key, option, request.USERDATA);
                return ProcessSubMenu(request, message);
            }

           

            return Ok(new UssdResponse
            {
                USERID = userid,
                MSISDN = request.MSISDN,
                MSG = "Welcome, VAG-OBIRI Lotteries:\n 1)Pioneer\n 2)Vag East\n3)Vag East\n4)African Lotto\n5)Obiri Special\n6)Old Soldier\n7)Sunday-Special",
                MSGTYPE = true
            });
        }

        private string GetSubmenus(string key,string optionValve, string option)
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
                    return $"{option}\n1.Direct-1\nEnter 1 number from (1-90)";
                //option 6
                case "61":
                case "62":
                case "63":
                case "64":
                case "65":
                case "66":
                case "67":
                    //case "7171":
                    return $"{option}\n6.Perm - 2 \nEnter 3 number from (1-90)";

                //option 7
                case "71":
                case "72":
                case "73":
                case "74":
                case "75":
                case "76":
                case "77":
                    //case "7171":
                    return $"{option}\n7.Perm - 3 \nEnter 4 number from (1-90)";



            }
            return $"{optionValve}:\n{option}.Direct-{option}\nEnter {option} numbers from (1-90).\n Separate each number with a space ";
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

        private async Task<IActionResult> ProcessFinalState(UssdRequest request, string message,string option)
        {
            var inputs = request.USERDATA.Split(" ");

            foreach (var input in inputs)
            {
                if (!int.TryParse(input, out int result))
                {
                    return Ok(new UssdResponse
                    {
                        USERID = userid,
                        MSISDN = request.MSISDN,
                        MSG = "Input value is not in the rigth format",
                        MSGTYPE = true
                    });
                }

                if (result > 90 || result < 1)
                {
                    return Ok(new UssdResponse
                    {
                        USERID = userid,
                        MSISDN = request.MSISDN,
                        MSG = "Entere value between 1 - 90",
                        MSGTYPE = true
                    });
                }
            }

            PreviousState.TryRemove(request.MSISDN, out UserState tt);
            var transaction = new UssdTransaction
            {
                Amount = 200,
                OptionName = option,
                OptionValue = request.USERDATA,
                PhoneNumber = request.MSISDN
            };
            _context.Add(transaction);
            await _context.SaveChangesAsync();

            var contact = new string(request.MSISDN.ToCharArray().Where(c => char.IsDigit(c)).ToArray());
            var _message = $"{message}:{transaction.Id}";
            var endPoint = $"https://apps.mnotify.net/smsapi?key=TOdkRPCFwgfCbusbKpMqyYnSn&to={contact}&msg={_message}&sender_id=VAG-OBIRI-LOTTERIES";
            using HttpClient client = new HttpClient();
            var response = await client.GetAsync(endPoint);

            return Ok(new UssdResponse
            {
                USERID = userid,
                MSISDN = request.MSISDN,
                MSG = "Success",
                MSGTYPE = false
            });
        }

        private IActionResult ProcessSubMenu(UssdRequest request,string message)
        {
            var _state = new UserState { CurrentState = state.CurrentState, PreviousData = request.USERDATA };
            PreviousState.TryRemove(request.MSISDN, out UserState tt);
            PreviousState.TryAdd(request.MSISDN, _state);
            return Ok(new UssdResponse
            {
                USERID = userid,
                MSISDN = request.MSISDN,
                MSG = message,
                MSGTYPE = true
            });
        }

        private IActionResult ProcessMenu(UssdRequest request, string message)
        {
            var _state = new UserState { CurrentState = "", PreviousData = request.USERDATA };
            PreviousState.TryAdd(request.MSISDN, _state);
            return Ok(new UssdResponse
            {
                USERID = userid,
                MSISDN = request.MSISDN,

                MSG = message,
                MSGTYPE = true
            });
        }
    }

    public  record MessageType
    {
        public string Message { get; set; }
        public string Option { get; set; }
    }

    public record UserState
    {
        public string CurrentState { get; set; }
        public string PreviousData { get; set; }
    }

    public class UssdRequest
    {
        public string USERID { get; set; }
        public string MSISDN { get; set; }
        public string USERDATA { get; set; }
        public bool MSGTYPE { get; set; }
        public string NETWORK { get; set; }
    }

    public class UssdResponse
    {
        public string USERID { get; set; }
        public string MSISDN { get; set; }
        public string MSG { get; set; }
        public bool MSGTYPE { get; set; }
    }
}
