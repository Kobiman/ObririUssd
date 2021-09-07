using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ObririUssd.Models
{
    public class UssdLock
    {
        public int Id { get; set; }
        public int StartTime { get; set; }
        public int EndTime { get; set; }
        public bool Disabled { get; set; }

        public bool DrawHasEnded() => DateTime.Now.Hour >= EndTime || DateTime.Now.Hour < StartTime;
    }
}
