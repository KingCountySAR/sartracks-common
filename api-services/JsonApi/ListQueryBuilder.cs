using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace SarData.Server.JsonApi
{
  public class ListQueryBuilder
  {
    public int DefaultPageSize { get; set; } = 10;

    public string DefaultSort { get; private set; }

    public ListQueryBuilder(string defaultSort)
    {
      DefaultSort = defaultSort;
    }

    public ListQueryStrategy Build(Dictionary<string, int> page, Dictionary<string, string> filter, string sort, IQueryCollection query)
    {
      if (!filter.ContainsKey(string.Empty) && query != null && query.TryGetValue("filter", out StringValues standaloneFilter) && standaloneFilter.Count == 1 && !string.IsNullOrWhiteSpace(standaloneFilter[0]))
      {
        filter.Add(string.Empty, standaloneFilter[0]);
      }

      page = page ?? new Dictionary<string, int>();

      if (!page.ContainsKey("size")) page.Add("size", 0);
      if (!page.ContainsKey("number")) page.Add("number", 1);

      if (page["number"] < 1) page["number"] = 1;
      if (page["size"] < 0) page["size"] = 10;

      return new ListQueryStrategy(page, filter, sort, DefaultSort);
    }
  }
}
