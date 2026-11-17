namespace BookingPlatformInsiders.Application.Payments;

public sealed class PaymentOptions
{
    public const string SectionName = "Payments";

    public string Provider { get; set; } = "mock";
    public int MaxAttempts { get; set; } = 3;
    public int PaymentTtlMinutes { get; set; } = 15;
}
