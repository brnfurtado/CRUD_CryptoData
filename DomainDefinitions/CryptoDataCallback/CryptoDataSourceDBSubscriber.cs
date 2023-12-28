using DomainDefinitions.Enums;
using DomainDefinitions.Interfaces;
using DomainDefinitions.Data;
using DomainDefinitions.AuxStaticCode;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using DomainDefinitions.Models;

namespace LocalTester
{
    public class CryptoDataSourceDBSubscriber : ICryptoDataCallback
    {
        public int cryptoCallbackID { get; }
        private Dictionary<string, List<(string, EFrequency)>> _subscribedCryptoPairs = new Dictionary<string, List<(string, EFrequency)>>();
        private AppDbContext _appDbContext;

        private CryptoDataSourceDBSubscriber(AppDbContext dbContext)
        {
            _appDbContext = dbContext;
        }

        //Using async method instead of constructor because C# does not allow for async constructors
        public static async Task<CryptoDataSourceDBSubscriber> CreateCryptoDataSourceDBSubscriber(AppDbContext dbContext)
        {
            CryptoDataSourceDBSubscriber cryptoDataSourceDBSubscriber = new CryptoDataSourceDBSubscriber(dbContext);

            return cryptoDataSourceDBSubscriber;
        }

        public async Task SubscribeCryptoData(string sourceName, string cryptoPair, EFrequency frequency)
        {
            ICryptoDataSource cryptoDataSource = await AuxStaticCode.GetDataSourceByName(sourceName);
            await cryptoDataSource.SubscribeCryptoData(cryptoPair, frequency, this);
        }

        public async Task UpdateSubscriptionCryptoData(string sourceName, string cryptoPair, EFrequency frequency)
        {
            ICryptoDataSource cryptoDataSource = await AuxStaticCode.GetDataSourceByName(sourceName);
            await cryptoDataSource.SubscribeCryptoData(cryptoPair, frequency, this);
        }

        public async Task UnsubscribeCryptoData(string sourceName, string cryptoPair, EFrequency frequency)
        {
            ICryptoDataSource cryptoDataSource = await AuxStaticCode.GetDataSourceByName(sourceName);
            await cryptoDataSource.UnsubscribeCryptoData(cryptoPair, frequency, this);
        }

        public Dictionary<string, List<string>> GetSubscribedPairs()
        {
            Dictionary<string, List<string>> returnDict = new Dictionary<string, List<string>>();

            foreach (var sourceName in _subscribedCryptoPairs)
            {
                returnDict.Add(sourceName.Key, new List<string>());
                foreach (var cryptoPair in sourceName.Value)
                {
                    returnDict[sourceName.Key].Add(cryptoPair.Item1);
                }
            }

            return returnDict;
        } 

        //Saves book data in the DB.
        public async Task OnDataUpdateCallback(string cryptoDataSourceName, string cryptoPair, decimal[] bookDataValues)
        {
            BookData bookData = BookData.CreateBookData(cryptoDataSourceName, cryptoPair, bookDataValues[0], bookDataValues[1], bookDataValues[2], bookDataValues[3]);

            _appDbContext.bookDatas.Add(bookData);
            await _appDbContext.SaveChangesAsync();
            //AuxStaticCode.Print($"[{DateTimeOffset.UtcNow}] Tester received [{cryptoDataSourceName}]-[{cryptoPair}] data with " +
            //    $"first values [{bookDataValues[1]},{bookDataValues[0]}] - [{bookDataValues[2]},{bookDataValues[3]}]");


        }
    }
}
