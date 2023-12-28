using DomainDefinitions.Enums;

namespace DomainDefinitions.Interfaces
{
    public interface ICryptoDataSourceFactory
    {
        Task<ICryptoDataSource> GetDataSourceByName(string sourceName);

    }

    public interface ICryptoDataSource
    {
        int cryptoDataSourceID { get; }
        List<string> listValidCryptoPairs { get; }

        void StartDataSource();

        Task<bool> SubscribeCryptoData(string cryptoPair, EFrequency frequency,  ICryptoDataCallback callback);

        Task<bool> UnsubscribeCryptoData(string cryptoPair, EFrequency frequency, ICryptoDataCallback callback);
        Task<bool> UpdateSubscriptionCryptoData(string cryptoPair, EFrequency frequency, ICryptoDataCallback callback);
    }

    public interface ICryptoDataCallback
    {
        int cryptoCallbackID { get; }
        Task OnDataUpdateCallback(string cryptoDataSourceName, string cryptoPair, decimal[] bookData);
    }
}