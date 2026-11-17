using BookingPlatformInsiders.Application.Payments.Abstractions;

namespace BookingPlatformInsiders.Application.Payments;

public interface IPaymentProviderResolver
{
    IPaymentProvider Current { get; }

    IPaymentProvider Resolve(string providerName);
}
