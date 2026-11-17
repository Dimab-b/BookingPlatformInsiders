using System.Net.Http.Json;
using BookingPlatformInsiders.Application.Common.Time;
using BookingPlatformInsiders.Application.Payments;
using BookingPlatformInsiders.Application.Payments.Abstractions;
using Microsoft.Extensions.Options;

namespace BookingPlatformInsiders.Infrastructure.Rates;

internal sealed class FrankfurterRateProvider : IRateProvider
{
    private readonly HttpClient _httpClient;
    private readonly IClock _clock;

    public FrankfurterRateProvider(HttpClient httpClient, IClock clock)
    {
        _httpClient = httpClient;
        _clock = clock;
    }

    public async Task<ExchangeRateQuote> GetEurRateAsync(
        string currency,
        CancellationToken cancellationToken = default)
    {
        var normalizedCurrency = currency.Trim().ToUpperInvariant();

        if (normalizedCurrency.Length != 3)
        {
            throw new RatesUnavailableException();
        }

        if (normalizedCurrency == "EUR")
        {
            return new ExchangeRateQuote(1m, "EUR", _clock.UtcNow);
        }

        try
        {
            var response = await _httpClient.GetAsync(
                $"latest?from={Uri.EscapeDataString(normalizedCurrency)}&to=EUR",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new RatesUnavailableException();
            }

            var payload = await response.Content.ReadFromJsonAsync<FrankfurterResponse>(cancellationToken);

            if (payload?.Rates is null
                || !payload.Rates.TryGetValue("EUR", out var rate)
                || rate <= 0)
            {
                throw new RatesUnavailableException();
            }

            return new ExchangeRateQuote(rate, "Frankfurter", _clock.UtcNow);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (RatesUnavailableException)
        {
            throw;
        }
        catch (Exception)
        {
            throw new RatesUnavailableException();
        }
    }

    private sealed record FrankfurterResponse(
        string Base,
        Dictionary<string, decimal>? Rates);
}
