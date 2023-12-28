using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DomainDefinitions.Models
{
    public class BookData
    {
        [Key]
        public long DateTimeUnixMS { get; set; }
        public string SourceName { get; set; }
        public string CryptoPair { get; set; }
        public decimal BidPrice { get; set; }
        public decimal BidSize { get; set; }
        public decimal AskPrice { get; set; }
        public decimal AskSize { get; set; }


        public static BookData CreateBookData(string sourceName, string cryptoPair,
            decimal bidPrice, decimal bidSize, decimal askPrice, decimal askSize)
        {
            BookData bookData = new BookData();

            bookData.DateTimeUnixMS = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            bookData.SourceName =  sourceName;
            bookData.CryptoPair = cryptoPair;
            bookData.BidPrice = bidPrice;
            bookData.BidSize = bidSize;
            bookData.AskPrice = askPrice;
            bookData.AskSize = askSize;

            return bookData;
        }
    }
}
