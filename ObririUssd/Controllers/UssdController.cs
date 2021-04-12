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
        static ConcurrentDictionary<string, MessageType> Messages = new();
        private UserState state = null;
        private string userid = "WEB_MATE";
        private UssdDataContext _context;
        public UssdController(UssdDataContext context)
        {
            _context = context;
            Messages.TryAdd("1", new MessageType { Message = "Mon.-PIONEER:\n1.Direct-1\n2.Direct-2\n3.Direct-3\n4.Direct-4\n5.Direct-5\n6.Perm 2\n7.Perm-3\n" });
            //previousData+Userdata+CurrentState
            Messages.TryAdd("1111", new MessageType { Message = "Mon.-PIONEER:\n1.Direct-1\nEnter number from (1-90)" });
            Messages.TryAdd("1211", new MessageType { Message = "Mon.-PIONEER:\n2.Direct-2\nEnter 2 numbers from (1-90).\n Separate each number with a space " });
            Messages.TryAdd("1311", new MessageType { Message = "Mon.-PIONEER:\n3.Direct-3\nEnter 3 numbers from (1-90).\n Separate each number with a space " });
            Messages.TryAdd("1411", new MessageType { Message = "Mon.-PIONEER:\n4.Direct-4\nEnter 4 numbers from (1-90).\n Separate each number with a space " });
            Messages.TryAdd("1511", new MessageType { Message = "Mon.-PIONEER:\n5.Direct-5\nEnter 5 numbers from (1-90).\n Separate each number with a space " });
            Messages.TryAdd("1611", new MessageType { Message = "Mon.-PIONEER:\n3.Direct-3\nEnter 3 numbers from (1-90).\n Separate each number with a space " });
            Messages.TryAdd("1711", new MessageType { Message = "Mon.-PIONEER:\n7.Perm-3\nEnter 4 numbers from (1-90).\n Separate each number with a space " });

            Messages.TryAdd("2", new MessageType { Message = "Mon.-PIONEER:\n1.Direct-1\n2.Direct-2\n3.Direct-3\n4.Direct-4\n5.Direct-5\n6.Perm 2\n7.Perm-3\n" });
            Messages.TryAdd("2121", new MessageType { Message = "Mon.-PIONEER:\n1.Direct-1\nEnter number from (1-90)" });
            Messages.TryAdd("2221", new MessageType { Message = "Mon.-PIONEER:\n2.Direct-2\nEnter 2 numbers from (1-90).\n Separate each number with a space " });
            Messages.TryAdd("2321", new MessageType { Message = "Mon.-PIONEER:\n3.Direct-3\nEnter 3 numbers from (1-90).\n Separate each number with a space " });
            Messages.TryAdd("2421", new MessageType { Message = "Mon.-PIONEER:\n4.Direct-4\nEnter 4 numbers from (1-90).\n Separate each number with a space " });
            Messages.TryAdd("2521", new MessageType { Message = "Mon.-PIONEER:\n5.Direct-5\nEnter 5 numbers from (1-90).\n Separate each number with a space " });
            Messages.TryAdd("2621", new MessageType { Message = "Mon.-PIONEER:\n3.Direct-3\nEnter 3 numbers from (1-90).\n Separate each number with a space " });
            Messages.TryAdd("2721", new MessageType { Message = "Mon.-PIONEER:\n7.Perm-3\nEnter 4 numbers from (1-90).\n Separate each number with a space " });

            Messages.TryAdd("3", new MessageType { Message = "Mon.-PIONEER:\n1.Direct-1\n2.Direct-2\n3.Direct-3\n4.Direct-4\n5.Direct-5\n6.Perm 2\n7.Perm-3\n" });
            Messages.TryAdd("3131", new MessageType { Message = "Mon.-PIONEER:\n1.Direct-1\nEnter number from (1-90)" });
            Messages.TryAdd("3231", new MessageType { Message = "Mon.-PIONEER:\n2.Direct-2\nEnter 2 numbers from (1-90).\n Separate each number with a space " });
            Messages.TryAdd("3331", new MessageType { Message = "Mon.-PIONEER:\n3.Direct-3\nEnter 3 numbers from (1-90).\n Separate each number with a space " });
            Messages.TryAdd("3431", new MessageType { Message = "Mon.-PIONEER:\n4.Direct-4\nEnter 4 numbers from (1-90).\n Separate each number with a space " });
            Messages.TryAdd("3531", new MessageType { Message = "Mon.-PIONEER:\n5.Direct-5\nEnter 5 numbers from (1-90).\n Separate each number with a space " });
            Messages.TryAdd("3631", new MessageType { Message = "Mon.-PIONEER:\n3.Direct-3\nEnter 3 numbers from (1-90).\n Separate each number with a space " });
            Messages.TryAdd("3731", new MessageType { Message = "Mon.-PIONEER:\n7.Perm-3\nEnter 4 numbers from (1-90).\n Separate each number with a space " });

            Messages.TryAdd("4", new MessageType { Message = "Mon.-PIONEER:\n1.Direct-1\n2.Direct-2\n3.Direct-3\n4.Direct-4\n5.Direct-5\n6.Perm 2\n7.Perm-3\n" });
            Messages.TryAdd("4141", new MessageType { Message = "Mon.-PIONEER:\n1.Direct-1\nEnter number from (1-90)" });
            Messages.TryAdd("4241", new MessageType { Message = "Mon.-PIONEER:\n2.Direct-2\nEnter 2 numbers from (1-90).\n Separate each number with a space " });
            Messages.TryAdd("4341", new MessageType { Message = "Mon.-PIONEER:\n3.Direct-3\nEnter 3 numbers from (1-90).\n Separate each number with a space " });
            Messages.TryAdd("4441", new MessageType { Message = "Mon.-PIONEER:\n4.Direct-4\nEnter 4 numbers from (1-90).\n Separate each number with a space " });
            Messages.TryAdd("4541", new MessageType { Message = "Mon.-PIONEER:\n5.Direct-5\nEnter 5 numbers from (1-90).\n Separate each number with a space " });
            Messages.TryAdd("4641", new MessageType { Message = "Mon.-PIONEER:\n3.Direct-3\nEnter 3 numbers from (1-90).\n Separate each number with a space " });
            Messages.TryAdd("4741", new MessageType { Message = "Mon.-PIONEER:\n7.Perm-3\nEnter 4 numbers from (1-90).\n Separate each number with a space " });

            //Final state
            //previousData+CurrentState
            Messages.TryAdd("1111", new MessageType { Message = "Your ticket: Monday - PIONEER:1.Direct - 1, 1GHS is registered for Direct - 1.Id", Option = "Monday - PIONEER:1.Direct - 1" });
            Messages.TryAdd("2111", new MessageType { Message = "Your ticket: Monday - PIONEER:2.Direct-2,  1GHS is registered for Direct - 2. Id", Option = "Monday - PIONEER:1.Direct - 2" });
            Messages.TryAdd("3111", new MessageType { Message = "Your ticket: Monday - PIONEER:3.Direct-3,  1GHS is registered for Direct - 3. Id", Option = "Monday - PIONEER:1.Direct - 3" });
            Messages.TryAdd("4111", new MessageType { Message = "Your ticket: Monday - PIONEER:3.Direct-4,  1GHS is registered for Direct - 4. Id", Option = "Monday - PIONEER:1.Direct - 4" });
            Messages.TryAdd("5111", new MessageType { Message = "Your ticket: Monday - PIONEER:5.Direct-5,  1GHS is registered for Direct - 5. Id", Option = "Monday - PIONEER:1.Direct - 5" });
            Messages.TryAdd("6111", new MessageType { Message = "Your ticket: Monday - PIONEER:6.Perm-2,  1GHS is registered for Perm - 2. Id", Option = "Monday - PIONEER:1.Direct - 6" });
            Messages.TryAdd("7111", new MessageType { Message = "Your ticket: Monday - PIONEER:7.Perm-2,  1GHS is registered for Perm - 2. Id", Option = "Monday - PIONEER:1.Direct - 7" });

            Messages.TryAdd("1211", new MessageType { Message = "Your ticket: Monday - PIONEER:1.Direct - 1, 1GHS is registered for Direct - 1.Id", Option = "Monday - PIONEER:1.Direct - 1" });
            Messages.TryAdd("2211", new MessageType { Message = "Your ticket: Monday - PIONEER:2.Direct-2,  1GHS is registered for Direct - 2. Id", Option = "Monday - PIONEER:1.Direct - 2" });
            Messages.TryAdd("3211", new MessageType { Message = "Your ticket: Monday - PIONEER:3.Direct-3,  1GHS is registered for Direct - 3. Id", Option = "Monday - PIONEER:1.Direct - 3" });
            Messages.TryAdd("4211", new MessageType { Message = "Your ticket: Monday - PIONEER:3.Direct-4,  1GHS is registered for Direct - 4. Id", Option = "Monday - PIONEER:1.Direct - 4" });
            Messages.TryAdd("5211", new MessageType { Message = "Your ticket: Monday - PIONEER:5.Direct-5,  1GHS is registered for Direct - 5. Id", Option = "Monday - PIONEER:1.Direct - 5" });
            Messages.TryAdd("6211", new MessageType { Message = "Your ticket: Monday - PIONEER:6.Perm-2,  1GHS is registered for Perm - 2. Id", Option = "Monday - PIONEER:1.Direct - 6" });
            Messages.TryAdd("7211", new MessageType { Message = "Your ticket: Monday - PIONEER:7.Perm-2,  1GHS is registered for Perm - 2. Id", Option = "Monday - PIONEER:1.Direct - 7" });

            Messages.TryAdd("1311", new MessageType { Message = "Your ticket: Monday - PIONEER:1.Direct - 1, 1GHS is registered for Direct - 1.Id", Option = "Monday - PIONEER:1.Direct - 1" });
            Messages.TryAdd("2311", new MessageType { Message = "Your ticket: Monday - PIONEER:2.Direct-2,  1GHS is registered for Direct - 2. Id", Option = "Monday - PIONEER:1.Direct - 2" });
            Messages.TryAdd("3311", new MessageType { Message = "Your ticket: Monday - PIONEER:3.Direct-3,  1GHS is registered for Direct - 3. Id", Option = "Monday - PIONEER:1.Direct - 3" });
            Messages.TryAdd("4311", new MessageType { Message = "Your ticket: Monday - PIONEER:3.Direct-4,  1GHS is registered for Direct - 4. Id", Option = "Monday - PIONEER:1.Direct - 4" });
            Messages.TryAdd("5311", new MessageType { Message = "Your ticket: Monday - PIONEER:5.Direct-5,  1GHS is registered for Direct - 5. Id", Option = "Monday - PIONEER:1.Direct - 5" });
            Messages.TryAdd("6311", new MessageType { Message = "Your ticket: Monday - PIONEER:6.Perm-2,  1GHS is registered for Perm - 2. Id", Option = "Monday - PIONEER:1.Direct - 6" });
            Messages.TryAdd("7311", new MessageType { Message = "Your ticket: Monday - PIONEER:7.Perm-2,  1GHS is registered for Perm - 2. Id", Option = "Monday - PIONEER:1.Direct - 7" });

            Messages.TryAdd("1411", new MessageType { Message = "Your ticket: Monday - PIONEER:1.Direct - 1, 1GHS is registered for Direct - 1.Id", Option = "Monday - PIONEER:1.Direct - 1" });
            Messages.TryAdd("2411", new MessageType { Message = "Your ticket: Monday - PIONEER:2.Direct-2,  1GHS is registered for Direct - 2. Id", Option = "Monday - PIONEER:1.Direct - 2" });
            Messages.TryAdd("3411", new MessageType { Message = "Your ticket: Monday - PIONEER:3.Direct-3,  1GHS is registered for Direct - 3. Id", Option = "Monday - PIONEER:1.Direct - 3" });
            Messages.TryAdd("4411", new MessageType { Message = "Your ticket: Monday - PIONEER:3.Direct-4,  1GHS is registered for Direct - 4. Id", Option = "Monday - PIONEER:1.Direct - 4" });
            Messages.TryAdd("5411", new MessageType { Message = "Your ticket: Monday - PIONEER:5.Direct-5,  1GHS is registered for Direct - 5. Id", Option = "Monday - PIONEER:1.Direct - 5" });
            Messages.TryAdd("6411", new MessageType { Message = "Your ticket: Monday - PIONEER:6.Perm-2,  1GHS is registered for Perm - 2. Id", Option = "Monday - PIONEER:1.Direct - 6" });
            Messages.TryAdd("7411", new MessageType { Message = "Your ticket: Monday - PIONEER:7.Perm-2,  1GHS is registered for Perm - 2. Id", Option = "Monday - PIONEER:1.Direct - 7" });

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
                Messages.TryGetValue(request.USERDATA, out MessageType m);
                return ProcessMenu(request, m.Message);
            }
            else if (state?.CurrentState.Length > 2)
            {
                Messages.TryGetValue(state.PreviousData + state.CurrentState, out MessageType m);
                return await ProcessFinalState(request, m.Message,m.Option);
            }
            else if(request.USERDATA is not "" && state?.CurrentState is not "")
            {
                Messages.TryGetValue(state.PreviousData+request.USERDATA+state.CurrentState, out MessageType m);
                return ProcessSubMenu(request, m.Message);
            }

           

            return Ok(new UssdResponse
            {
                USERID = userid,
                MSISDN = request.MSISDN,
                MSG = "Welcome to VAG-OBIRI Lotteries :\n 1)Mon-Pioneer\n 2)Tue-Vag East\n3)Wed-Vag West\n4)Thur-African Lotto\n5)Fri-Obiri Special\n6)Sat-Old Soldier\n7)Sunday-Special",
                MSGTYPE = true
            });
        }

        private async Task<IActionResult> ProcessFinalState(UssdRequest request, string message,string option)
        {
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
