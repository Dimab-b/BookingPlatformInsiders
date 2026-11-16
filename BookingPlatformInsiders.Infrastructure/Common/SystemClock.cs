using BookingPlatformInsiders.Application.Common.Time;

namespace BookingPlatformInsiders.Infrastructure.Common;

internal sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
