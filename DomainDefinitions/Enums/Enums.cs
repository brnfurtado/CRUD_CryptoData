namespace DomainDefinitions.Enums
{
    public enum EFrequency
    {
        //Using multiples so the values can be accounted for on the subsequent bigger values
        OneSecond = 1 * 1000,
        FiveSeconds = 5 * 1000,
        ThirtySeconds = 30 * 1000,
        OneMinute = 60 * 1000,
        FiveMinute = 5 * 60 * 1000,
        OneHour = 60 * 60 * 1000,
        Daily = 24 * 60 * 60 * 1000,
    }
}