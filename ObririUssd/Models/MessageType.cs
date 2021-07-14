using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ObririUssd.Models
{
    public record MessageType
    {
        public string Message { get; set; }
        public string Option { get; set; }
    }
}
