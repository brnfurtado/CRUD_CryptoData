using DomainDefinitions.Factories;
using DomainDefinitions.Interfaces;
using System.Collections.Concurrent;

namespace DomainDefinitions.AuxStaticCode
{
    public static class AuxStaticCode
    {
        //Not allowing prints in release due to wanting a better performance
#if DEBUG
        public static bool allowPrint { get; private set; } = true;
#else
        public static bool allowPrint { get; private set; } = false;
#endif

        //Hardcoded for now, but can be implemented specifically for each exchange.
        public static List<string> validCryptoPairs = new List<string>
            {
                "BTCUSDT",
                "ETHUSDT",
                "XRPUSDT",
                "ADAUSDT",
                "SOLUSDT",
            };

        //used when there's not enough depth
        public static double defaultBookValue = -1;

        //Ensuring all the code refrences this factory, to guarantee that it's the only instanciated factory as a singleton
        private static ICryptoDataSourceFactory cryptoDataSourceFactory = CryptoDataSourceFactory.GetCryptoDataSourceFactory();

        //Ensuring all the code refrences this DBCallback, to guarantee that it's the only instanciated DBCallback as a singleton
        public static ICryptoDataCallback cryptoDataSourceDBSubscriber;

        //Min and max rows allowed to be returned on the API.
        public static int minReturnRows = 1;
        public static int maxReturnRows = 100;

        public static void Print(string printMessage, bool overridePrint = false)
        {
            if (overridePrint || allowPrint)
            {
                Console.WriteLine(printMessage);
            }
        }

        public static async Task<ICryptoDataSource> GetDataSourceByName(string sourceName)
        {
            //int ID = StaticDicts.StaticDicts.cryptoDataSourceNameToID[sourceName];

            return await cryptoDataSourceFactory.GetDataSourceByName(sourceName);
        }

        //Allowing a public method to set crypto data source, in order not to need to reference the factory
        public static void SetCryptoDataSourceFactory(ICryptoDataSourceFactory factory)
        {
            cryptoDataSourceFactory = factory;
        }

        //Theere might be a workaround to hardcoding these, maybe loading all the possible classes in the "Implementations folder".
        public static List<string> validExchanges = new List<string>
        {
            "Binance",
            "Coinbase"
        };


        //Used to check if the ID provided by the user for a subscription is unique.
        public static List<int> RegisteredIDs = new List<int>();
    }
}