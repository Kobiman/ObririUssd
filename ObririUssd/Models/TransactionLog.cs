using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ObririUssd.Models
{
    public class TransactionLog
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int TotalDraws { get; set; }
        public float TotalDrawsAmount { get; set; }
        public int WiningDrawAmount { get; set; }
    }
}
