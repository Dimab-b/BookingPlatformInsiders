namespace BookingPlatformInsiders.Application.Common.Pagination;

public static class PaginationNormalizer
{
    public const int DefaultPageSize = 50;
    public const int MaxPageSize = 100;

    public static (int PageNumber, int PageSize, int Offset) Normalize(int pageNumber, int pageSize)
    {
        var normalizedPageNumber = Math.Max(1, pageNumber);
        var normalizedPageSize = Math.Clamp(
            pageSize <= 0 ? DefaultPageSize : pageSize,
            1,
            MaxPageSize);

        var offsetInLong = (long)(normalizedPageNumber - 1) * normalizedPageSize;
        var offset = offsetInLong > int.MaxValue ? int.MaxValue : (int)offsetInLong;

        return (normalizedPageNumber, normalizedPageSize, offset);
    }
}
