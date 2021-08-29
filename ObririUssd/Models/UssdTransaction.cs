using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ObririUssd.Models
{
    public class UssdTransaction
    {
        public int Id { get; set; }
        public string PhoneNumber { get; set; }
        public string OptionName { get; set; }
        public string OptionValue { get; set; }
        public float Amount { get; set; }
        public float WinningAmount { get; set; }
        public bool Win { get; set; }
        public bool PaymentStatus { get; set; }
        public bool Proccessed { get; set; }
        public DateTime TransactionDate { get; set; } = DateTime.Now;
    }
}
