using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ObririUssd.Models
{
    public record UserState
    {
        public string CurrentState { get; set; }
        public string PreviousData { get; set; }
        public string SelectedValues { get; set; }
        public string UserOption { get; set; }
        public string GameType { get; set; }
        public DateTime Duration { get; set; } = DateTime.Now.AddSeconds(30);
    }
}
