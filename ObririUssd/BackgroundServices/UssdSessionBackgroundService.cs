using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ObririUssd.BackgroundServices
{
    public class UssdSessionBackgroundService : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(25000, stoppingToken);
                foreach(var state in UssdSessionManager.PreviousState)
                {
                    if (state.Value.Duration <= DateTime.Now)
                    {
                        UssdSessionManager.PreviousState.TryRemove(state);
                    }
                }
            } 
        }
    }
}
