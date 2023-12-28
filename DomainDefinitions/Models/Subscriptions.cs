using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel;

namespace DomainDefinitions.Models
{
    public class Subscription
    {
        [Key]
        public int ID { get; set; }

        [DefaultValue("Binance")]
        public string SourceName { get; set; }

        [DefaultValue("BTCUSDT")]
        public string CryptoPair { get; set; }

        [DefaultValue(30000)]
        public int FrequencyMS { get; set; }
    }
}
