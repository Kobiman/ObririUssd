using ObririUssd.Models;
using System.Collections.Concurrent;

namespace ObririUssd
{
    public class UssdSessionManager
    {
        public static ConcurrentDictionary<string, UserState> _previousState;
        public static ConcurrentDictionary<string, UserState> PreviousState = _previousState ?? new ConcurrentDictionary<string, UserState>();
    }
}
