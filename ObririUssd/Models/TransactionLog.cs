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
        public int WiningDraws { get; set; }
        public string MachineNumbers { get; set; }
        public float WiningDrawAmount { get; set; }
        public string GameType { get; set; }
    }
}
