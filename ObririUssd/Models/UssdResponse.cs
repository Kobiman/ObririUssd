using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ObririUssd.Models
{
    public class UssdResponse
    {
        [JsonProperty("sessionID")]
        public string SessionID { get; set; }
        [JsonProperty("userID")]
        public string UserID { get; set; }
        [JsonProperty("continueSession")]
        public bool ContinueSession { get; set; }
        [JsonProperty("msisdn")]
        public string Msisdn { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }
       

        //Arkesel
    //      public string SessionID { get; set; }
    // public string UserID { get; set; }
    // public bool ContinueSession { get; set; }
    // public string Msisdn { get; set; }
    // public string Message { get; set; }

        //

        public static UssdResponse CreateResponse(string userid, string MSISDN, string message,bool continueSession)
        {
            return new UssdResponse
            {
                UserID = userid,
                Msisdn = MSISDN,
                Message = message,
                ContinueSession = continueSession
                // MSGTYPE = MSGTYPE
            };
        }
    }
}
