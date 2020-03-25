using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Newtonsoft.Json.Serialization;

namespace SarData.Server.JsonApi
{
  public class ProjectionLibrary
  {
    private readonly Dictionary<string, LambdaExpression> modelProjections;
    private readonly Dictionary<string, object> constants;
    private readonly NamingStrategy naming;

    public ProjectionLibrary(Dictionary<string, LambdaExpression> modelProjections, Dictionary<string, object> constants, NamingStrategy naming)
    {
      this.modelProjections = modelProjections;
      this.constants = constants;
      this.naming = naming;
    }



    public Expression<Func<F, T>> BuildProjection<F, T>(string[] include, Dictionary<string, string[]> fields)
    {
      if (!modelProjections.TryGetValue($"{typeof(F).Name}-{typeof(T).Name}", out LambdaExpression expr))
      {
        throw new InvalidOperationException($"No projection found to go from {typeof(F).Name} to {typeof(T).Name}");
      }

      Expression<Func<F, T>> typedExpr = (Expression<Func<F, T>>)new ProjectionVisitor(include, fields, modelProjections, constants, naming).Visit(expr);
      return typedExpr;
    }

    public static T GetProjected<F, T>(Expression<Func<F>> inputGetter)
    {
      return default(T);
    }

    public static Func<F, T> GetProjection<F, T>()
    {
      return (a) => default(T);
    }

    public static T GetConstant<T>(string key)
    {
      return default(T);
    }
  }
}
