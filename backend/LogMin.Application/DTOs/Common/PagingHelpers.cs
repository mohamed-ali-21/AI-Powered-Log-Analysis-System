namespace LogMin.Application.DTOs.Common;

public static class PagingHelpers
{
    public const int DefaultPageSize = 50;
    public const int MaxPageSize = 200;

    public static (int Skip, int Take, int Page, int PageSize) Normalize(int page, int pageSize)
    {
        var p = page < 1 ? 1 : page;
        var ps = pageSize switch
        {
            < 1 => DefaultPageSize,
            > MaxPageSize => MaxPageSize,
            _ => pageSize
        };
        return ((p - 1) * ps, ps, p, ps);
    }
}
