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
            if (PreviousState.TryGetValue(request.MSISDN, out state))
            {
                PreviousState.TryRemove(request.MSISDN, out UserState tt);
                var _state = tt with { CurrentState = tt.CurrentState+"1" };
                PreviousState.TryAdd(request.MSISDN, _state);
                PreviousState.TryGetValue(request.MSISDN, out state);
            }

            if (state is null && request.USERDATA == "1")
            {
                var _state = new UserState { CurrentState = request.USERDATA, PreviousData = request.USERDATA };
                PreviousState.TryAdd(request.MSISDN, _state);
                return Ok(new UssdResponse
                {
                    USERID = userid,
                    MSISDN = request.MSISDN,

                    MSG = "Mon.-PIONEER:\n1.Direct-1\n2.Direct-2\n3.Direct-3\n4.Direct-4\n5.Direct-5\n6.Perm 2\n7.Perm-3\n",
                    MSGTYPE = true
                });
            }

            if (state?.CurrentState == "11" && request?.USERDATA == "1")
            {
                var _state = new UserState { CurrentState = state.CurrentState, PreviousData = request.USERDATA };
                PreviousState.TryRemove(request.MSISDN, out UserState tt);
                PreviousState.TryAdd(request.MSISDN, _state);
                return Ok(new UssdResponse
                {
                    USERID = userid,
                    MSISDN = request.MSISDN,
                    MSG = "Mon.-PIONEER:\n1.Direct-1\nEnter number from (1-90)",
                    MSGTYPE = true
                });
            }

            //Final state for Monday-PIONEER------------------------------------------------------------------------------------------------------------------------------>
            if (state?.CurrentState == "111" && state?.PreviousData == "1")
            {
                PreviousState.TryRemove(request.MSISDN, out UserState tt);
                var transaction = new UssdTransaction
                {
                    Amount = 200,
                    OptionName = "Monday-PIONEER:1.Direct-1",
                    OptionValue = request.USERDATA,
                    PhoneNumber = request.MSISDN
                };
                _context.Add(transaction);
                await _context.SaveChangesAsync();

                var contact = new string(request.MSISDN.ToCharArray().Where(c => char.IsDigit(c)).ToArray());
                var message = "Your ticket: Monday - PIONEER:1.Direct - 1,  1GHS is registered for Direct - 1. TkT : 123";
                var endPoint = $"https://apps.mnotify.net/smsapi?key=UdmAJsUYQodKElhwb6uJCB4lA&to={contact}&msg={message}&sender_id=UENR";
                using HttpClient client = new HttpClient();
                var response = await client.GetAsync(endPoint);

                return Ok(new UssdResponse
                {
                    USERID = userid,
                    MSISDN = request.MSISDN,
                    MSG = "Success",
                    MSGTYPE =false
                });
            }
            //Main Option 1 sub 2---------------------------------------------------------------------------------------------------------------------
            if (state?.CurrentState == "11" && request?.USERDATA == "2")
            {
                PreviousState.TryRemove(request.MSISDN, out UserState tt);
                var _state = new UserState { CurrentState = state.CurrentState, PreviousData = request.USERDATA };
                PreviousState.TryAdd(request.MSISDN, _state);
                return Ok(new UssdResponse
                {
                    USERID = userid,
                    MSISDN = request.MSISDN,
                    MSG = "Mon.-PIONEER:\n2.Direct-2\nEnter 2 numbers from (1-90).\n Separate each number with a space ",
                    MSGTYPE = true
                });
            }

            //Final state for Monday-PIONEER
            if (state?.CurrentState == "111" && state?.PreviousData == "2")
            {
                //var state = new UserState { CurrentState = request.USERDATA, PreviousData = request.USERDATA };
                PreviousState.TryRemove(request.MSISDN, out UserState tt);
                var transaction = new UssdTransaction
                {
                    Amount = 200,
                    OptionName = "Monday-PIONEER:2.Direct-2",
                    OptionValue = request.USERDATA,
                    PhoneNumber = request.MSISDN
                };
                _context.Add(transaction);
                await _context.SaveChangesAsync();

                var contact = new string(request.MSISDN.ToCharArray().Where(c => char.IsDigit(c)).ToArray());
                var message = "Your ticket: Monday - PIONEER:2.Direct-2,  1GHS is registered for Direct - 2. TkT : 123";
                var endPoint = $"https://apps.mnotify.net/smsapi?key=UdmAJsUYQodKElhwb6uJCB4lA&to={contact}&msg={message}&sender_id=UENR";
                using HttpClient client = new HttpClient();
                var response = await client.GetAsync(endPoint);

                return Ok(new UssdResponse
                {
                    USERID = userid,
                    MSISDN = request.MSISDN,
                    MSG = "Success",
                    MSGTYPE =false
                });
            }

            ////Main Option 1 sub 3-----------------------------------------------------------------------------------------------------------------------------------
            //if (state == "11" && request.USERDATA == "3")
            //{
            //    PreviousState.TryAdd(request.MSISDN, request.USERDATA);
            //    return Ok(new UssdResponse
            //    {
            //        USERID = userid,
            //        MSISDN = request.MSISDN,
            //        MSG = "Mon.-PIONEER:\n3.Direct-3\nEnter 3 numbers from (1-90).\n Separate each number with a space ",
            //        MSGTYPE = true
            //    });
            //}

            ////Final state for Monday-PIONEER
            //if (state == "111")
            //{
            //    PreviousState.TryRemove(request.MSISDN, out string tt);
            //    var transaction = new UssdTransaction
            //    {
            //        Amount = 200,
            //        OptionName = "Monday-PIONEER:3.Direct-3",
            //        OptionValue = request.USERDATA,
            //        PhoneNumber = request.MSISDN
            //    };
            //    _context.Add(transaction);
            //    await _context.SaveChangesAsync();

            //    var contact = new string(request.MSISDN.ToCharArray().Where(c => char.IsDigit(c)).ToArray());
            //    var message = "Your ticket: Monday - PIONEER:3.Direct-3,  1GHS is registered for Direct - 3. TkT : 123";
            //    var endPoint = $"https://apps.mnotify.net/smsapi?key=UdmAJsUYQodKElhwb6uJCB4lA&to={contact}&msg={message}&sender_id=UENR";
            //    using HttpClient client = new HttpClient();
            //    var response = await client.GetAsync(endPoint);

            //    return Ok(new UssdResponse
            //    {
            //        USERID = userid,
            //        MSISDN = request.MSISDN,
            //        MSG = "Success",
            //        MSGTYPE = false
            //    });
            //}

            ////Main Option 1 sub 4---------------------------------------------------------------------------------------------------------------------------
            //if (state == "11" && request.USERDATA == "4")
            //{
            //    PreviousState.TryAdd(request.MSISDN, request.USERDATA);
            //    return Ok(new UssdResponse
            //    {
            //        USERID = userid,
            //        MSISDN = request.MSISDN,
            //        MSG = "Mon.-PIONEER:\n4.Direct-4\nEnter 4 numbers from (1-9).\n Separate each number with a space ",
            //        MSGTYPE = true
            //    });
            //}


            //if (state == "111")
            //{
            //    PreviousState.TryRemove(request.MSISDN, out string tt);
            //    var transaction = new UssdTransaction
            //    {
            //        Amount = 200,
            //        OptionName = "Monday-PIONEER:4.Direct-4",
            //        OptionValue = request.USERDATA,
            //        PhoneNumber = request.MSISDN
            //    };
            //    _context.Add(transaction);
            //    await _context.SaveChangesAsync();

            //    var contact = new string(request.MSISDN.ToCharArray().Where(c => char.IsDigit(c)).ToArray());
            //    var message = "Your ticket: Monday - PIONEER:2.Direct-4,  1GHS is registered for Direct - 4. TkT : 123";
            //    var endPoint = $"https://apps.mnotify.net/smsapi?key=UdmAJsUYQodKElhwb6uJCB4lA&to={contact}&msg={message}&sender_id=UENR";
            //    using HttpClient client = new HttpClient();
            //    var response = await client.GetAsync(endPoint);

            //    return Ok(new UssdResponse
            //    {
            //        USERID = userid,
            //        MSISDN = request.MSISDN,
            //        MSG = "Success",
            //        MSGTYPE = false
            //    });
            //}

            ////option 1 sub 5---------------------------------------------------------------------------------------------------------------------------------
            //if (state == "11" && request.USERDATA == "5")
            //{
            //    PreviousState.TryAdd(request.MSISDN, request.USERDATA);
            //    return Ok(new UssdResponse
            //    {
            //        USERID = userid,
            //        MSISDN = request.MSISDN,
            //        MSG = "Mon.-PIONEER:\n5.Direct-5\nEnter 5 numbers from (1-9).\n Separate each number with a space ",
            //        MSGTYPE = true
            //    });
            //}


            //if (state == "111")
            //{
            //    PreviousState.TryRemove(request.MSISDN, out string tt);
            //    var transaction = new UssdTransaction
            //    {
            //        Amount = 200,
            //        OptionName = "Monday-PIONEER:5.Direct-5",
            //        OptionValue = request.USERDATA,
            //        PhoneNumber = request.MSISDN
            //    };
            //    _context.Add(transaction);
            //    await _context.SaveChangesAsync();

            //    var contact = new string(request.MSISDN.ToCharArray().Where(c => char.IsDigit(c)).ToArray());
            //    var message = "Your ticket: Monday - PIONEER:5.Direct-5,  1GHS is registered for Direct - 5. TkT : 123";
            //    var endPoint = $"https://apps.mnotify.net/smsapi?key=UdmAJsUYQodKElhwb6uJCB4lA&to={contact}&msg={message}&sender_id=UENR";
            //    using HttpClient client = new HttpClient();
            //    var response = await client.GetAsync(endPoint);

            //    return Ok(new UssdResponse
            //    {
            //        USERID = userid,
            //        MSISDN = request.MSISDN,
            //        MSG = "Success",
            //        MSGTYPE = false
            //    });
            //}
            ////Option 1 sub 6------------------------------------------------------------------------------------------------------------------------------------
            //if (state == "11" && request.USERDATA == "6")
            //{
            //    PreviousState.TryAdd(request.MSISDN, request.USERDATA);
            //    return Ok(new UssdResponse
            //    {
            //        USERID = userid,
            //        MSISDN = request.MSISDN,
            //        MSG = "Mon.-PIONEER:\n6.Perm-2\nEnter 3 numbers from (1-90).\n Separate each number with a space ",
            //        MSGTYPE = true
            //    });
            //}


            //if (state == "111")
            //{
            //    PreviousState.TryRemove(request.MSISDN, out string tt);
            //    var transaction = new UssdTransaction
            //    {
            //        Amount = 200,
            //        OptionName = "Monday-PIONEER:6.Perm-2",
            //        OptionValue = request.USERDATA,
            //        PhoneNumber = request.MSISDN
            //    };
            //    _context.Add(transaction);
            //    await _context.SaveChangesAsync();

            //    var contact = new string(request.MSISDN.ToCharArray().Where(c => char.IsDigit(c)).ToArray());
            //    var message = "Your ticket: Monday - PIONEER:6 Perm 2,  1GHS is registered for Perm 2. TkT : 123";
            //    var endPoint = $"https://apps.mnotify.net/smsapi?key=UdmAJsUYQodKElhwb6uJCB4lA&to={contact}&msg={message}&sender_id=UENR";
            //    using HttpClient client = new HttpClient();
            //    var response = await client.GetAsync(endPoint);

            //    return Ok(new UssdResponse
            //    {
            //        USERID = userid,
            //        MSISDN = request.MSISDN,
            //        MSG = "Success",
            //        MSGTYPE = false
            //    });
            //}

            ////option 1sub 7--------------------------------------------------------------------------------------------------------------------------

            //if (state == "11" && request.USERDATA == "7")
            //{
            //    PreviousState.TryAdd(request.MSISDN, request.USERDATA);
            //    return Ok(new UssdResponse
            //    {
            //        USERID = userid,
            //        MSISDN = request.MSISDN,
            //        MSG = "Mon.-PIONEER:\n6.Perm-3\nEnter 4 numbers from (1-90).\n Separate each number with a space ",
            //        MSGTYPE = true
            //    });
            //}


            //if (state == "111")
            //{
            //    PreviousState.TryRemove(request.MSISDN, out string tt);
            //    var transaction = new UssdTransaction
            //    {
            //        Amount = 200,
            //        OptionName = "Monday-PIONEER:7.Perm-3",
            //        OptionValue = request.USERDATA,
            //        PhoneNumber = request.MSISDN
            //    };
            //    _context.Add(transaction);
            //    await _context.SaveChangesAsync();

            //    var contact = new string(request.MSISDN.ToCharArray().Where(c => char.IsDigit(c)).ToArray());
            //    var message = "Your ticket: Monday - PIONEER:7 Perm 3,  1GHS is registered for Perm 3. TkT : 123";
            //    var endPoint = $"https://apps.mnotify.net/smsapi?key=UdmAJsUYQodKElhwb6uJCB4lA&to={contact}&msg={message}&sender_id=UENR";
            //    using HttpClient client = new HttpClient();
            //    var response = await client.GetAsync(endPoint);

            //    return Ok(new UssdResponse
            //    {
            //        USERID = userid,
            //        MSISDN = request.MSISDN,
            //        MSG = "Success",
            //        MSGTYPE = false
            //    });
            //}

            //if (string.IsNullOrWhiteSpace(state) && request.USERDATA == "2")
            //{
            //    PreviousState.TryAdd(request.USERID, request.USERDATA);
            //    return Ok(new UssdResponse
            //    {
            //        USERID = request.USERID,
            //        MSISDN = request.MSISDN,
            //        MSG = "Tuesday-VAG EAST:\n1.Direct-1\n2.Direct-2\n3.Direct-3\n4.Direct-4\n5.Direct-5\n6.Perm 2\n7.Perm-3",
            //        MSGTYPE =true
            //    });
            //}

            //if (state == "21" && request.USERDATA == "1")
            //{
            //    PreviousState.TryAdd(request.USERID, request.USERDATA);
            //    return Ok(new UssdResponse
            //    {
            //        USERID = request.USERID,
            //        MSISDN = request.MSISDN,
            //        MSG = "Monday-PIONEER:\n1.Direct-1\nEnter number from (1-9)",
            //        MSGTYPE = true
            //    });
            //}

            //if (state == "211")
            //{
            //    PreviousState.TryRemove(request.USERID, out string tt);
            //    return Ok(new UssdResponse
            //    {
            //        USERID = request.USERID,
            //        MSISDN = request.MSISDN,
            //        MSG = "Success",
            //        MSGTYPE = true
            //    });
            //}

            //if (string.IsNullOrWhiteSpace(state) && request.USERDATA == "3")
            //{
            //    PreviousState.TryAdd(request.USERID, request.USERDATA);
            //    return Ok(new UssdResponse
            //    {
            //        USERID = request.USERID,
            //        MSISDN = request.MSISDN,
            //        MSG = "Wednesday-VAG WEST:\n1.Direct-1\n2.Direct-2\n3.Direct-3\n4.Direct-4\n5.Direct-5\n6.Perm 2\n7.Perm-3",
            //        MSGTYPE = true
            //    });
            //}

            //if (state == "31" && request.USERDATA == "1")
            //{
            //    PreviousState.TryAdd(request.USERID, request.USERDATA);
            //    return Ok(new UssdResponse
            //    {
            //        USERID = request.USERID,
            //        MSISDN = request.MSISDN,
            //        MSG = "Monday-PIONEER:\n1.Direct-1\nEnter number from (1-9)",
            //        MSGTYPE = true
            //    });
            //}

            //if (state == "311")
            //{
            //    PreviousState.TryRemove(request.USERID, out string tt);
            //    return Ok(new UssdResponse
            //    {
            //        USERID = userid,
            //        MSISDN = request.MSISDN,
            //        MSG = "Success",
            //        MSGTYPE = false
            //    });
            //}

            if (!PreviousState.TryGetValue(request.MSISDN, out state) && request.USERDATA != string.Empty)
            {
                var state = new UserState { CurrentState = request.USERDATA, PreviousData = request.USERDATA };
                PreviousState.TryAdd(request.MSISDN, state);
            }

            return Ok(new UssdResponse
            {
                USERID = userid,
                MSISDN = request.MSISDN,
                MSG = "Welcome to Obiri Lotteries :\n 1)Mon-Pioneer\n 2)Tue-Vag East\n3)Wed-Vag West\n4)Thur-African Lotto\n5)Fri-Obiri Special\n6)Sat-Old Soldier\n7)Sunday-Special",
                MSGTYPE = true
            });
        }
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
