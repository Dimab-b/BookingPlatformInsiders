namespace BookingPlatformInsiders.Domain.Bookings;

public sealed record BookingPrice
{
    public const int MaxRateProviderLength = 50;
    public const int MaxAmountDecimals = 2;
    public const int MaxRateDecimals = 8;

    public decimal AmountOriginal { get; }
    public string Currency { get; }
    public decimal AmountEur { get; }
    public decimal ExchangeRate { get; }
    public string RateProvider { get; }
    public DateTime RateCapturedAtUtc { get; }

    public BookingPrice(
        decimal amountOriginal,
        string currency,
        decimal amountEur,
        decimal exchangeRate,
        string rateProvider,
        DateTime rateCapturedAtUtc)
    {
        if (amountOriginal <= 0)
        {
            throw new InvalidBookingPriceException();
        }

        if (string.IsNullOrWhiteSpace(currency))
        {
            throw new InvalidBookingCurrencyException();
        }

        var normalizedCurrency = currency.Trim().ToUpperInvariant();

        if (normalizedCurrency.Length != 3
            || !normalizedCurrency.All(character => character is >= 'A' and <= 'Z'))
        {
            throw new InvalidBookingCurrencyException();
        }

        if (amountEur <= 0 || decimal.Round(amountEur, MaxAmountDecimals) != amountEur)
        {
            throw new InvalidBookingPriceException();
        }

        if (exchangeRate <= 0 || decimal.Round(exchangeRate, MaxRateDecimals) != exchangeRate)
        {
            throw new InvalidBookingExchangeRateException();
        }

        if (string.IsNullOrWhiteSpace(rateProvider)
            || rateProvider.Trim().Length > MaxRateProviderLength)
        {
            throw new InvalidBookingRateProviderException();
        }

        if (rateCapturedAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new InvalidBookingRateCapturedAtException();
        }

        AmountOriginal = amountOriginal;
        Currency = normalizedCurrency;
        AmountEur = amountEur;
        ExchangeRate = exchangeRate;
        RateProvider = rateProvider.Trim();
        RateCapturedAtUtc = rateCapturedAtUtc;
    }
}
