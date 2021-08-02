﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ObririUssd.Models
{
    public class UssdRequest
    {
        public string USERID { get; set; }
        public string MSISDN { get; set; }
        public string USERDATA { get; set; }
        public bool MSGTYPE { get; set; }
        public string NETWORK { get; set; }

        private int result;
        public bool ValidateInputFormat() => int.TryParse(USERDATA, out result);
        public bool ValidateInputFormats()
        {
            var result = true;
            foreach(var r in USERDATA.Split(" "))
            {
                result = result &&int.TryParse(r, out int tt);
            }
            return result;
        }

        public bool ValidateInputRange(int max, int min) => result <= max && result >= min;
        public bool ValidateInputRanges(int max, int min)
        {
            var value = true;
            foreach (var r in USERDATA.Split(" "))
            {
                int.TryParse(r, out int tt);
                value = value && tt <= max && tt >= min;
            }
            return value;
        }
        public Task<HttpResponseMessage> ProcessPayment()
        {
            var json = JsonSerializer.Serialize(
                                    new PaymentRequest
                                    {
                                        amount = To12Digits(USERDATA),
                                        processing_code = "000200",
                                        transaction_id = Unique_Code(),
                                        desc = "Mobile Money Payment Test",
                                        merchant_id = "TTM-00005781",
                                        subscriber_number = MSISDN,
                                        r_switch = NETWORK
                                    });
            HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            //client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json; charset=utf-8");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", 
                Convert.ToBase64String(Encoding.UTF8.GetBytes("vag60e5c12178f1f:ZGNlNDY2ODRlNmUzODRlZTQ4MTMxZTdkYWZiZjNlZDI=")));
            client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };
            
            return client.PostAsync("https://test.theteller.net/v1.1/transaction/process", content);
        }

        private string Unique_Code()
        {

            var chars = "0123456789";
            var stringChars = new char[12];
            var random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }
            return new string(stringChars);
        }

        private string To12Digits(string amount)
        {
            StringBuilder sb = new StringBuilder();
            int i = 0;
            while (i<(12 - amount.Length))
            {
                sb.Append('0');
                i++;
            }
            return sb.Append(amount).ToString();
        }
    }
}