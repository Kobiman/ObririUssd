using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ObririUssd.Models
{
    public class UssdTransaction
    {
        public int Id { get; set; }

        public int TSN { get; set; }
        public string PhoneNumber { get; set; }
        public string OptionName { get; set; }
        public string OptionValue { get; set; }
        public float Amount { get; set; }
        public float WinningAmount { get; set; }
        public bool Win { get; set; }
        
        public bool PaymentStatus { get; set; }
        public string ApprovedBy { get; set; }
        public bool Proccessed { get; set; }
        public DateTime TransactionDate { get; set; } = DateTime.Now;
        public string Message { get; set; }
        public bool Status { get; set; }
        public string GameType { get; set; }
    }
}
