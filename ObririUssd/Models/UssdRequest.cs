using RestSharp;
using System;
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
        public bool HasDuplicate()
        {
            var temp = USERDATA.Split(" ");
            if (temp.Distinct().Count().Equals(temp.Length)) return false;
            return true;
        }
        //public Task<HttpResponseMessage> ProcessPayment()
        //{
        //    var json = JsonSerializer.Serialize(
        //                            new PaymentRequest
        //                            {
        //                                amount = To12Digits(USERDATA),
        //                                processing_code = "000200",
        //                                transaction_id = Unique_Code(),
        //                                desc = "Mobile Money Payment Test",
        //                                merchant_id = "TTM-00005781",
        //                                subscriber_number = MSISDN,
        //                                r_switch = NETWORK
        //                            });
        //    HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
        //    var client = new HttpClient();
        //    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
        //    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        //    //client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json;");// charset=utf-8
        //    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", 
        //        Convert.ToBase64String(Encoding.UTF8.GetBytes("vag60e5c12178f1f:ZGNlNDY2ODRlNmUzODRlZTQ4MTMxZTdkYWZiZjNlZDI=")));
        //    client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };

        //    return client.PostAsync("https://test.theteller.net/v1.1/transaction/process", content);
        //}

        public Task<IRestResponse> ProcessPayment()
        {
            //var client = new RestClient("https://test.theteller.net/v1.1/transaction/process"); 
            var client = new RestClient("https://prod.theteller.net/v1.1/transaction/process");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            //request.AddHeader("Authorization", "Basic dmFnNjBlNWMxMjE3OGYxZjpaR05sTkRZMk9EUmxObVV6T0RSbFpUUTRNVE14WlRka1lXWmlaak5sWkRJPQ==");
            request.AddHeader("Authorization", "Basic dmFnNjBlNWMxMjE3OGYxZjpZemc0TVRZMk4ySXlZelZqTVRBMVkySTBPRGM1TVdOaFpUSXhNREF6T0RZPQ==");
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Cookie", "PHPSESSID=ckou37ehci5nuclea2hld275bg");
            var body = "";
            if (NETWORK == "VODAFONE")
            {
                body = JsonSerializer.Serialize(
                                       new PaymentRequestForVodafone
                                       {
                                           amount = To12Digits(USERDATA),
                                           processing_code = "000200",
                                           transaction_id = Unique_Code(),
                                           desc = "VAG OBIRI LOTORIES",
                                           merchant_id = "TTM-00005781",
                                           subscriber_number = MSISDN,
                                           r_switch = GetNetwork(NETWORK)
                                       });
            }
            else
            {
                body = JsonSerializer.Serialize(
                                        new PaymentRequest
                                        {
                                            amount = To12Digits(USERDATA),
                                            processing_code = "000200",
                                            transaction_id = Unique_Code(),
                                            desc = "VAG OBIRI LOTORIES",
                                            merchant_id = "TTM-00005781",
                                            subscriber_number = MSISDN,
                                            r_switch = GetNetwork(NETWORK),
                                            voucher_code = Unique_Code()
                                        });
            }
            
            request.AddParameter("application/json", body, ParameterType.RequestBody);
            return client.ExecuteAsync(request);
        }

        public Task<IRestResponse> ProcesPyment()
        {
            var client = new RestClient("https://prod.theteller.net/v1.1/transaction/process");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Authorization", "Basic dmFnNjBlNWMxMjE3OGYxZjpZemc0TVRZMk4ySXlZelZqTVRBMVkySTBPRGM1TVdOaFpUSXhNREF6T0RZPQ==");
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Cookie", "PHPSESSID=ckou37ehci5nuclea2hld275bg");
            var body = @"{
" + "\n" +
            @" ""amount"" : ""_amount"",
" + "\n" +
            @" ""processing_code"" : ""000200"",
" + "\n" +
            @" ""transaction_id"" : ""_transaction_id"",
" + "\n" +
            @" ""desc"" : ""VAG OBIRI LOTORIES"",
" + "\n" +
            @" ""merchant_id"" : ""TTM-00005781"",
" + "\n" +
            @" ""subscriber_number"" : ""_subscriber_number"",
" + "\n" +
            @" ""r-switch"" : ""_r-switch""
" + "\n" +
            @"}";
            request.AddParameter("application/json", body, ParameterType.RequestBody);
            return client.ExecuteAsync(request);
        }

        public int GetNumberOfLines(string previousData, int length)
        {
            if (previousData == "6") return GetPerm2Lines(length);
            return GetPerm3Lines(length);
        }

        private int GetPerm2Lines(int length)
        {
            int numberofLines = 0;
            if (length == 2) return 1;
            for (int i = 2; i < length; i++)
            {
                numberofLines += i;
            }
            return numberofLines + 1;
        }
        private int GetPerm3Lines(int length)
        {
            int numberofLines = 4;
            int divisor = 2;
            if (length == 3) return 1;
            if (length == 4) return 4;
            for (int i = 4; i < length; i++)
            {
                numberofLines = numberofLines * i/divisor;
                divisor++;
            }
            return numberofLines;
        }

        private string GetNetwork(string network)
        {
           if (network.ToUpper().Equals("MTN")) return "MTN";
           if (network.ToUpper().Equals("AIRTELTIGO")) return "ATL";
           if (network.ToUpper().Equals("TIGO")) return "TGO";
           if (network.ToUpper().Equals("VODAFONE")) return "VDF";
            return "";
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
            var _amount = float.Parse(amount) * 100;
            StringBuilder sb = new StringBuilder();
            int i = 0;
            while (i<(12 - _amount.ToString().Length))
            {
                sb.Append('0');
                i++;
            }
            return sb.Append(_amount).ToString();
        }

        public Dictionary<string, string> GameTypes()=>new Dictionary<string, string>
        {
            { "PIONEER", "VAGOBIRIGames" },
            { "MONDAY SPECIAL", "NLAGames" },
            { "VAG EAST", "VAGOBIRIGames" },
            { "LUCKY TUESDAY", "NLAGames" },
            { "VAG WEST", "VAGOBIRIGames" },
            { "MID-WEEK", "NLAGames" },
            { "AFRICAN LOTTO", "VAGOBIRIGames" },
            { "FORTUNE THURSDAY", "NLAGames" },
            { "OBIRI SPECIAL", "VAGOBIRIGames" },
            { "FRIDAY BONANZA", "NLAGames" },
            { "OLD SOLDIER", "VAGOBIRIGames" },
            { "NATIONAL", "NLAGames" },
            { "SUNDAY SPECIAL", "VAGOBIRIGames" }
        };
    }
}
