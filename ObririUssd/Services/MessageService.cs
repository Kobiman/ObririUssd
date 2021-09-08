using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ObririUssd.Services
{
    public class MessageService
    {

        public async Task SendSms(string contact, string message)
        {
            var _contact = new string(contact.ToCharArray().Where(c => char.IsDigit(c)).ToArray());
            var endPoint = $"https://apps.mnotify.net/smsapi?key=TOdkRPCFwgfCbusbKpMqyYnSn&to={_contact}&msg={message}&sender_id=VAG-OBIRI";
            using HttpClient client = new HttpClient();
            var response = await client.GetAsync(endPoint);
        }
    }
}
