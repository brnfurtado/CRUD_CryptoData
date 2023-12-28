using DomainDefinitions.CryptoDataSubscriber.AbstractClasses;
using DomainDefinitions.Enums;
using System.Globalization;
using System.Text.Json;

namespace DomainDefinitions.CryptoDataSubscriber.Implementations
{
    public class Coinbase_DataSubscriber : AbstractDataSubscriber<Coinbase_DataSubscriber>
    {
        private const string connectionProtocol = "https://";
        private const string HOST = "api.exchange.coinbase.com";
        private const string bookDepthPath = "/products/{0}/book";


        //Required for singleton implementation
        public static Coinbase_DataSubscriber CreateInstance()
        {
            lock (lockInstance)
            {
                if (instance == null)
                {
                    instance = new Coinbase_DataSubscriber();
                    instance.SetSelfNameAndID("Coinbase", 2);
                }
                return instance;
            }
        }

        protected override async Task<decimal[]> ImplementationGetCryptoData(string cryptoPair)
        {
            decimal[] returnBook = new decimal[4];

            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                //Setting any header because it's required
                httpClient.DefaultRequestHeaders.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; " +
                                  "Windows NT 5.2; .NET CLR 1.0.3705;)");

                HttpResponseMessage responseMessage = await httpClient.GetAsync(GetBookHTTPURI(bookDepthPath, cryptoPair));

                var stringResponse = await responseMessage.Content.ReadAsStringAsync();

                HTTPBookDepthResponse response = JsonSerializer.Deserialize<HTTPBookDepthResponse>(stringResponse);

                returnBook[0] = decimal.Parse(((JsonElement)response.bids[0][0]).GetString(), CultureInfo.InvariantCulture);
                returnBook[1] = decimal.Parse(((JsonElement)response.bids[0][1]).GetString(), CultureInfo.InvariantCulture);
                returnBook[2] = decimal.Parse(((JsonElement)response.asks[0][0]).GetString(), CultureInfo.InvariantCulture);
                returnBook[3] = decimal.Parse(((JsonElement)response.asks[0][1]).GetString(), CultureInfo.InvariantCulture);
            }
            return returnBook;
        }

        protected override string GetBookHTTPURI(string bookDepthPath, string cryptoPair)
        {
            return connectionProtocol + HOST + string.Format(bookDepthPath, ConvertInternalToExternalCryptoPairSymbol(cryptoPair)) + "?level=2";
        }

        protected override string ConvertExternalToInternalCryptoPairSymbol(string cryptoPair)
        {
            return cryptoPair.Substring(0, 3) + cryptoPair.Substring(4, cryptoPair.Length - 3);
        }

        protected override string ConvertInternalToExternalCryptoPairSymbol(string cryptoPair)
        {
            return cryptoPair.Substring(0, 3) + "-" + cryptoPair.Substring(3, cryptoPair.Length - 3);
        }

        //Has to be public for UTF8Json to parse it
        public class HTTPBookDepthResponse
        {
            public List<List<object>> bids { get; set; }
            public List<List<object>> asks { get; set; }
            public long sequence { get; set; }
            public bool auction_mode { get; set; }
            public object auction { get; set; }
            public DateTime time { get; set; }
        }
    }
}