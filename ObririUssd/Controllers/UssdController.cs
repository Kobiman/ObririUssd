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
        ConcurrentDictionary<string, string> Options = new();
        private UserState state = null;
        private string userid = "WEB_MATE";
        private UssdDataContext _context;
        public UssdController(UssdDataContext context)
        {
            _context = context;
            Options.TryAdd("1", "Mon.-PIONEER");
            Options.TryAdd("2", "Tue.-VAG EAST");
            Options.TryAdd("3", "Wed.-VAG EAST");
            Options.TryAdd("4", "Thur.-AFRICAN LOTTO");
            Options.TryAdd("5", "Fri.-OBIRI FRIDAY");
            Options.TryAdd("6", "Sat.-OLD SOLDIER");
            Options.TryAdd("7", "SUN.-SPECIAL");
        }

        [HttpPost("index")]
        public async Task<IActionResult> Index([FromBody] UssdRequest request)
        {
            if (PreviousState.TryGetValue(request.MSISDN, out state))
            {
                PreviousState.TryRemove(request.MSISDN, out UserState tt);
                var _state = tt with { CurrentState = tt.CurrentState+"1" };
                PreviousState.TryAdd(request.MSISDN, _state);
                PreviousState.TryGetValue(request.MSISDN, out state);
            }

            if(request.USERDATA is not "" && state is null)
            {
                Options.TryGetValue(request.USERDATA, out string option);
                var message = $"{option}\n1.Direct-1\n2.Direct-2\n3.Direct-3\n4.Direct-4\n5.Direct-5\n6.Perm 2\n7.Perm-3\n";
                return ProcessMenu(request, message);
            }
            else if (state?.CurrentState.Length > 2)
            {
                Options.TryGetValue(request.USERDATA, out string option);
                var key = state.PreviousData;
                var m = GetFinalStates(key, option, request.USERDATA);
                return await ProcessFinalState(request, m.Message, m.Option);
            }
            else if(request.USERDATA is not "" && state?.CurrentState is not "")
            {
                Options.TryGetValue(request.USERDATA, out string option);
                //previousData+Userdata+CurrentState
                var key = state.PreviousData+request.USERDATA+state.CurrentState;
                var message = GetSubmenus(key, option, request.USERDATA);
                return ProcessSubMenu(request, message);
            }

           

            return Ok(new UssdResponse
            {
                USERID = userid,
                MSISDN = request.MSISDN,
                MSG = "Welcome, VAG-OBIRI Lotteries:\n 1)Mon-Pioneer\n 2)Tue-Vag East\n3)Wed-Vag West\n4)Thur-African Lotto\n5)Fri-Obiri Special\n6)Sat-Old Soldier\n7)Sunday-Special",
                MSGTYPE = true
            });
        }

        private string GetSubmenus(string key,string optionValve, string option)
        {
            switch (key)
            {
                //previousData+Userdata+CurrentState
                case "1111":
                case "2121":
                case "3131":
                case "4141":
                case "5151":
                case "6161":
                case "7171":
                    return $"{option}\n1.Direct-1\nEnter 1 number from (1-90)";
                //option 6
                case "1611":
                case "2621":
                case "3631":
                case "4641":
                case "5651":
                case "6661":
                case "7671":
                    //case "7171":
                    return $"{option}\n6.Perm - 2 \nEnter 3 number from (1-90)";

                //option 7
                case "1711":
                case "2721":
                case "3731":
                case "4741":
                case "5751":
                case "6761":
                case "7771":
                    //case "7171":
                    return $"{option}\n7.Perm - 3 \nEnter 4 number from (1-90)";



            }
            return $"{optionValve}:\n{option}.Direct-{option}\nEnter {option} numbers from (1-90).\n Separate each number with a space ";
        }

        private MessageType GetFinalStates(string key, string optionValve, string option)
        {
            switch (key)
            {
                //previousData+Userdata+CurrentState
                case "6":
                    return new MessageType { Message = $"Your ticket: {optionValve}:{option}.Perm-2,  1GHS is registered for Perm - 2. Id", Option = $"{optionValve} - Perm 2" };
                case "7":
                    return new MessageType { Message = $"Your ticket: {optionValve}:{option}.Perm-3,  1GHS is registered for Perm - 3. Id", Option = $"{optionValve} - Perm 3" };
            }
            return new MessageType { Message = $"Your ticket: {optionValve}:{option}.Direct-{option},  1GHS is registered for Direct - {option}. Id", Option = $"{optionValve}:{option}.Direct - {option}" };
        }

        private async Task<IActionResult> ProcessFinalState(UssdRequest request, string message,string option)
        {
            ////int x = 30;
            //if (Enumerable.Range(1, 100).Contains(x))
            //    //true

            //    if (x >= 1 && x <= 100)

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
            var endPoint = $"https://apps.mnotify.net/smsapi?key=TOdkRPCFwgfCbusbKpMqyYnSn&to={contact}&msg={_message}&sender_id=VAG-OBIRI";
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
            var _state = new UserState { CurrentState = request.USERDATA, PreviousData = request.USERDATA };
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
