using DomainDefinitions.CryptoDataSubscriber.AbstractClasses;
using DomainDefinitions.Enums;
using System.Globalization;
using System.Text.Json;

namespace DomainDefinitions.CryptoDataSubscriber.Implementations
{
    public class Binance_DataSubscriber : AbstractDataSubscriber<Binance_DataSubscriber>
    {
        private const string connectionProtocol = "https://";
        private const string HOST = "api.binance.com";
        private const string bookDepthPath = "/api/v3/depth";


        //Required for singleton implementation
        public static Binance_DataSubscriber CreateInstance()
        {
            lock (lockInstance)
            {
                if (instance == null)
                {
                    instance = new Binance_DataSubscriber();
                    instance.SetSelfNameAndID("Binance", 1);
                }
                return instance;
            }
        }

        protected override async Task<decimal[]> ImplementationGetCryptoData(string cryptoPair)
        {
            decimal[] returnBook = new decimal[4];

            using (HttpClient httpClient = new HttpClient())
            {
                HttpResponseMessage responseMessage = await httpClient.GetAsync(GetBookHTTPURI(bookDepthPath, cryptoPair));

                var stringResponse = await responseMessage.Content.ReadAsStringAsync();

                HTTPBookDepthResponse response = JsonSerializer.Deserialize<HTTPBookDepthResponse>(stringResponse);

                returnBook[0] = decimal.Parse(response.bids[0][0], CultureInfo.InvariantCulture);
                returnBook[1] = decimal.Parse(response.bids[0][1], CultureInfo.InvariantCulture);
                returnBook[2] = decimal.Parse(response.asks[0][0], CultureInfo.InvariantCulture);
                returnBook[3] = decimal.Parse(response.asks[0][1], CultureInfo.InvariantCulture);
            }

            return returnBook;
        }


        protected override string GetBookHTTPURI(string bookDepthPath, string cryptoPair)
        {
            return connectionProtocol + HOST + bookDepthPath + "?limit=1&symbol=" + ConvertInternalToExternalCryptoPairSymbol(cryptoPair);
        }


        protected override string ConvertExternalToInternalCryptoPairSymbol(string cryptoPair)
        {
            return cryptoPair;
        }


        protected override string ConvertInternalToExternalCryptoPairSymbol(string cryptoPair)
        {
            return cryptoPair;
        }

        //Has to be public for UTF8Json to parse it
        public class HTTPBookDepthResponse
        {
            public long lastUpdateId { get; set; }
            public List<List<string>> bids { get; set; }
            public List<List<string>> asks { get; set; }
        }
    }
}