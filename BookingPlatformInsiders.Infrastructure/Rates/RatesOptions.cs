namespace BookingPlatformInsiders.Infrastructure.Rates;

public sealed class RatesOptions
{
    public const string SectionName = "Rates";

    public string BaseUrl { get; set; } = "https://api.frankfurter.app/";
    public int TimeoutSeconds { get; set; } = 10;
}
