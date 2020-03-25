using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SarData
{
  public static class LinqExtensions
  {
    private static PropertyInfo GetPropertyInfo(Type objType, string name)
    {
      var properties = objType.GetProperties();
      var matchedProperty = properties.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
      if (matchedProperty == null)
        throw new ArgumentException("name");

      return matchedProperty;
    }
    private static LambdaExpression GetOrderExpression(Type objType, PropertyInfo pi)
    {
      var paramExpr = Expression.Parameter(objType);
      var propAccess = Expression.PropertyOrField(paramExpr, pi.Name);
      var expr = Expression.Lambda(propAccess, paramExpr);
      return expr;
    }

    public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> query, string name)
    {
      Type t = query.GetType();
      string methodName = query.IsOrdered() ? "ThenBy" : "OrderBy";
      if (name[0] == '-')
      {
        methodName += "Descending";
        name = name.Substring(1);
      }

      var propInfo = GetPropertyInfo(typeof(T), name);
      var expr = GetOrderExpression(typeof(T), propInfo);

      var method = typeof(Queryable).GetMethods().FirstOrDefault(m => m.Name == methodName && m.GetParameters().Length == 2);
      var genericMethod = method.MakeGenericMethod(typeof(T), propInfo.PropertyType);
      return (IOrderedQueryable<T>)genericMethod.Invoke(null, new object[] { query, expr });
    }

    public static IOrderedQueryable<T> ApplySort<T>(this IQueryable<T> query, string sortList, string defaultSort)
    {
      if (defaultSort.Contains(',')) throw new InvalidOperationException("doesn't support multiple default sorts yet");

      defaultSort = defaultSort.ToLowerInvariant();
      bool hasDefaultSort = false;
      if (!string.IsNullOrWhiteSpace(sortList))
      {
        foreach (var s in sortList.Split(','))
        {
          query = query.OrderBy(s);
          hasDefaultSort |= s.ToLowerInvariant().TrimStart('-') == defaultSort.ToLowerInvariant().TrimStart('-');
        }
      }
      if (!hasDefaultSort) query = query.OrderBy(defaultSort);
      return (IOrderedQueryable<T>)query;
    }

    public static bool IsOrdered<T>(this IQueryable<T> queryable)
    {
      if (queryable == null)
      {
        throw new ArgumentNullException(nameof(queryable));
      }

      return queryable.Expression.Type == typeof(IOrderedQueryable<T>);
    }
  }
}
