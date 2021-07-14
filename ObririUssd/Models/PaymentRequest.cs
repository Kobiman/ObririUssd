using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ObririUssd.Models
{
    public class PaymentRequest
    {
        public string amount { get; set; }
        public string processing_code { get; set; }
        public string transaction_id { get; set; }
        public string desc { get; set; }
        public string merchant_id { get; set; }
        public string subscriber_number { get; set; }
        [JsonPropertyName("r-switch")]
        public string r_switch { get; set; }
}
}
