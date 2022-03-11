using ObririUssd.Data;
using ObririUssd.Models;
using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace ObririUssd
{
    public interface IPaymentChannel
    {
        IAsyncEnumerable<PaymentChannelMessage> ReadAllAsync();
        Task WriteAsync(PaymentChannelMessage request);
    }

    public class PaymentChannel : IPaymentChannel
    {
        private readonly Channel<PaymentChannelMessage> Requests = Channel.CreateUnbounded<PaymentChannelMessage>();

        public async Task WriteAsync(PaymentChannelMessage request)
        {
            await Requests.Writer.WriteAsync(request);
        }

        public IAsyncEnumerable<PaymentChannelMessage> ReadAllAsync()
        {
            return Requests.Reader.ReadAllAsync();
        }
    }

    public record PaymentChannelMessage(UssdRequest Request, UserState state);
}
