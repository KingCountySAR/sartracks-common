using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SarData.Server.JsonApi
{
  public class ListQueryStrategy
  {
    private readonly Dictionary<string, int> page;
    private readonly Dictionary<string, string> filter;
    private readonly string sort;
    private readonly string defaultSort;

    public ListQueryStrategy(Dictionary<string, int> page, Dictionary<string, string> filter, string sort, string defaultSort)
    {
      this.page = page;
      this.filter = filter;
      this.sort = sort;
      this.defaultSort = defaultSort;
    }

    public async Task<object> Run<Q, P>(
      IQueryable<Q> unfilteredQuery,
      Func<string, Expression<Func<Q, bool>>> searchBuilder,
      Expression<Func<Q, P>> projection,
      Action<P> localProcessor
      )
    {
      var query = ApplyFilters(unfilteredQuery, searchBuilder);

      var accountList = await ApplyPaging(ApplySort(query)).Select(projection).ToListAsync();

      var totalCount = await unfilteredQuery.CountAsync();
      int filteredCount = totalCount;
      if (query != unfilteredQuery)
      {
        filteredCount = await query.CountAsync();
      }

      if (localProcessor != null)
      {
        foreach (var row in accountList)
        {
          localProcessor(row);
        }
      }

      return new
      {
        Meta = new { TotalRows = totalCount, FilteredRows = filteredCount },
        Data = accountList
      };
    }

    private IQueryable<T> ApplyFilters<T>(IQueryable<T> original, Func<string, Expression<Func<T, bool>>> searchBuilder)
    {
      if (filter.TryGetValue("", out string globalFilter) && !string.IsNullOrWhiteSpace(globalFilter))
      {
        return original.Where(searchBuilder(globalFilter));
      }
      return original;
    }

    private IOrderedQueryable<T> ApplySort<T>(IQueryable<T> original)
    {
      return original.ApplySort(sort, defaultSort);
    }

    private IQueryable<T> ApplyPaging<T>(IQueryable<T> query)
    {
      if (page["size"] > 0)
      {
        if (page["number"] > 1)
        {
          query = query.Skip((page["number"] - 1) * page["size"]);
        }
        query = query.Take(page["size"]);
      }
      return query;
    }
  }
}
