using Services.Core.Models;

namespace Services.Core.Helpers;

public static class PaginationHelper
{
    public static IQueryable<T> ApplyPaging<T>(this IQueryable<T> query, PagedRequestQuery paginationParameters)
    {
        // todo: This has been validated using the PageGreaterThanZeroAttribute
        var pageNumber = paginationParameters.PageNumber <= 0 ? 1 : paginationParameters.PageNumber;

        return query
            .Skip((pageNumber - 1) * paginationParameters.PageSize)
            .Take(paginationParameters.PageSize);
    }
}
