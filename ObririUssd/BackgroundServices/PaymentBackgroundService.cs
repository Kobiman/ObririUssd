using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ObririUssd.Data;
using ObririUssd.Models;
using ObririUssd.Services;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ObririUssd.BackgroundServices
{
    public class PaymentBackgroundService : BackgroundService
    {
        private UssdDataContext _context;
        private readonly IPaymentChannel _channel;
        //private readonly IServiceCollection _provider;
        public PaymentBackgroundService(IPaymentChannel channel)
        {
            _channel = channel;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var r in _channel.ReadAllAsync())
            {
                await Task.Run(async () =>
                {
                    var response = await r.Request.ProcessPayment();
                    if (response.Content.ToUpper().Contains("approved".ToUpper()))
                    {
                        var mainMenuItem = OptionsOfTheDay[DaysOfTheWeek[DateTime.Now.DayOfWeek.ToString()]];
                        mainMenuItem.TryGetValue(r.state.UserOption, out string optionName);
                        var m = GetFinalStates(r.state.PreviousData, optionName, r.Request.USERDATA);
                        await ProcessFinalState(r.Request, m.Message, m.Option, r.state.SelectedValues);
                    }
                }, stoppingToken).ConfigureAwait(false);
            }
        }

        private async Task ProcessFinalState(UssdRequest request, string message, string option, string optionValue)
        {
            try { 
            var transaction = new UssdTransaction
            {
                Amount = int.Parse(request.USERDATA),
                OptionName = option,
                OptionValue = optionValue,
                GameType = GameTypes[option.Split(":")[0]],
                PhoneNumber = request.MSISDN
            };

            using (var scope = ServiceContext.ServiceProvider())
            {
                _context = scope.ServiceProvider.GetRequiredService<UssdDataContext>();
                _context.Add(transaction);
                await _context.SaveChangesAsync();
                var savedTransaction = await _context.Trans.FindAsync(transaction.Id);
                savedTransaction.Message = $"{message}:{transaction.Id} {DateTime.Now}";
                savedTransaction.Status = true;
                await _context.SaveChangesAsync();
            }                
            await new MessageService().SendSms(request.MSISDN, $"{message}:{transaction.Id}. Selected values {optionValue}\n{DateTime.Now}");
           }
            catch (Exception ex) { }
        }       

        Dictionary<string, string> DaysOfTheWeek = new Dictionary<string, string>
        {
            { "Monday", "1" },{ "Tuesday", "2" },{ "Wednesday", "3" },{ "Thursday", "4" },{ "Friday", "5" },{ "Saturday", "6" },{ "Sunday", "7" }
        };
        Dictionary<string, string> OptionsOfTheWeek = new Dictionary<string, string>
        {
            { "1", "1. PIONEER\n2. MONDAY SPECIAL" },{ "2", "1. VAG EAST\n2. LUCKY TUESDAY" },{ "3", "1. VAG WEST\n 2. MID-WEEK" },{ "4", "1. AFRICAN LOTTO\n2. FORTUNE THURSDAY" },{ "5", "1. OBIRI SPECIAL\n2. FRIDAY BONANZA" },{ "6", "1. OLD SOLDIER\n2. NATIONAL" },{ "7", "1. SUNDAY SPECIAL" }
        };
        Dictionary<string, Dictionary<string, string>> OptionsOfTheDay = new Dictionary<string, Dictionary<string, string>>
        {
            { "1", new Dictionary<string, string>{{ "1", "PIONEER" },{ "2", "MONDAY SPECIAL" }}},
            { "2", new Dictionary<string, string>{{ "1", "VAG EAST" },{ "2", "LUCKY TUESDAY" }}},
            { "3", new Dictionary<string, string>{{ "1", "VAG WEST" },{ "2", "MID-WEEK" }}},
            { "4", new Dictionary<string, string>{{ "1", "AFRICAN LOTTO" },{ "2", "FORTUNE THURSDAY" }}},
            { "5", new Dictionary<string, string>{{ "1", "OBIRI SPECIAL" },{ "2", "FRIDAY BONANZA" }}},
            { "6", new Dictionary<string, string>{{ "1", "OLD SOLDIER" },{ "2", "NATIONAL" }}},
            { "7", new Dictionary<string, string>{{ "1", "SUNDAY SPECIAL" }}}
        };

        private MessageType GetFinalStates(string previousValue, string optionName, string amount)
        {
            switch (previousValue)
            {
                //previousData+Userdata+CurrentState
                case "6":
                    return new MessageType { Message = $"Your ticket: {optionName}:Perm-2,  GHS {amount} is registered for Perm - 2. Id", Option = $"{optionName}:Perm - 2" };
                case "7":
                    return new MessageType { Message = $"Your ticket: {optionName}:Perm-3,  GHS {amount} is registered for Perm - 3. Id", Option = $"{optionName}:Perm - 3" };
            }
            return new MessageType { Message = $"Your ticket: {optionName} Direct-{previousValue},  GHS {amount} is registered for Direct - {previousValue}. Id", Option = $"{optionName}:Direct - {previousValue}" };
        }
    }
}
