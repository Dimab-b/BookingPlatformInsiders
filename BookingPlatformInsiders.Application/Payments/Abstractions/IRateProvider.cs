namespace BookingPlatformInsiders.Application.Payments.Abstractions;

public sealed record ExchangeRateQuote(
    decimal EurPerUnit,
    string Provider,
    DateTime CapturedAtUtc);

public interface IRateProvider
{
    Task<ExchangeRateQuote> GetEurRateAsync(
        string currency,
        CancellationToken cancellationToken = default);
}
