using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace DomainDefinitions.Models
{
    public class RetrieveBookDataObject
    {
        [DefaultValue("[\"Binance\", \"Coinbase\"]")]
        public List<string> SourceNames { get; set; }

        [DefaultValue("[\"BTCUSDT\", \"ETHUSDT\", \"XRPUSDT\", \"SOLUSDT\", \"ADAUSDT\"]")]
        public List<string> CryptoPairs { get; set; }

        [DefaultValue(10)]
        public int LimitRows { get; set; }
    }
}
