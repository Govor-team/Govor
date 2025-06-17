using Microsoft.EntityFrameworkCore;

namespace Govor.Core.Infrastructure.Extensions;

public static class QueryableExtensions
{
    public static async Task<List<T>> ToListOrThrowIfEmpty<T>(this IQueryable<T> query, Exception ex)
    {
        var list = await query.ToListAsync();
        if (list.Count == 0) throw ex;
        return list;
    }
}
