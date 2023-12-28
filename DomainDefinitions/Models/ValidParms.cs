using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using DomainDefinitions.Enums;

namespace DomainDefinitions.Models
{
    public class ValidParamsObject
    {
        public string ID { get; set; } = "Unique Int value, necessary to edit or delete the subscription after added.";
        public List<string> validSourceNames { get; set; } = new List<string> { "Coinbase", "Binance" };
        public List<EFrequency>  validFrequenciesMS { get; set; } = new List<EFrequency>{
            EFrequency.OneSecond, EFrequency.FiveSeconds, EFrequency.ThirtySeconds, EFrequency.OneMinute, EFrequency.FiveMinute, EFrequency.OneHour, EFrequency.Daily};

        public List<string> validCryptoPairs { get; set; } = AuxStaticCode.AuxStaticCode.validCryptoPairs;

        public string LimitRows { get; set; } = "Min value is 1, Max value is 100";
    }
}
