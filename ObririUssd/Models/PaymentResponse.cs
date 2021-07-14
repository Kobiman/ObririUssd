using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ObririUssd.Models
{
    public class PaymentResponse
    {
        //public string transaction_id { get; set; }
        public string status { get; set; }
        public int code { get; set; }
        public string description { get; set; }
    }
    //"{\"status\":\"failed\",\"code\":979,\"description\":\"Error: Content type is not set or is not application\\/json\"}"
}
