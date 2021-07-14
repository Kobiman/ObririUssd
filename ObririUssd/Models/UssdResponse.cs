using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ObririUssd.Models
{
    public class UssdResponse
    {
        public string USERID { get; set; }
        public string MSISDN { get; set; }
        public string MSG { get; set; }
        public bool MSGTYPE { get; set; }

        public static UssdResponse CreateResponse(string userid, string MSISDN, string message,bool MSGTYPE)
        {
            return new UssdResponse
            {
                USERID = userid,
                MSISDN = MSISDN,
                MSG = message,
                MSGTYPE = MSGTYPE
            };
        }
    }
}
