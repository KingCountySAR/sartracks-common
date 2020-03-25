using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Newtonsoft.Json.Serialization;
using SarData.JsonApi;
using SarData.Server.Apis.JsonApi;

namespace SarData.Server.JsonApi
{
  internal class ProjectionVisitor : ExpressionVisitor
  {
    private readonly string[] include;
    private readonly Dictionary<string, string[]> fields;
    private readonly Dictionary<string, LambdaExpression> projections;
    private readonly Dictionary<string, object> constants;
    private readonly NamingStrategy naming;
    Stack<string> path = new Stack<string>();
    Stack<ParameterExpression> root = new Stack<ParameterExpression>();

    public ProjectionVisitor(string[] include, Dictionary<string, string[]> fields, Dictionary<string, LambdaExpression> projections, Dictionary<string, object> constants, NamingStrategy naming)
    {
      this.include = include;
      this.fields = fields;
      this.projections = projections;
      this.constants = constants;
      this.naming = naming;
    }

    public override Expression Visit(Expression node)
    {
      if (root.Count == 0 && node is LambdaExpression)
      {
        root.Push(((LambdaExpression)node).Parameters?[0]);
      }
      return base.Visit(node);
    }

    protected override Expression VisitNew(NewExpression node)
    {
      return base.VisitNew(node);
    }

    private Expression GetDateTimeConstant(DateTime date)
    {
      Expression<Func<DateTime>> closure = () => date;
      return closure.Body;
    }

    private Expression GetDateTimeOffsetConstant(DateTimeOffset date)
    {
      Expression<Func<DateTimeOffset>> closure = () => date;
      return closure.Body;
    }

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
      MethodInfo mi = node.Method;
      if (mi.DeclaringType == typeof(ProjectionLibrary) && mi.IsStatic)
      {
        if (mi.Name == "GetConstant")
        {
          string key = (string)((node.Arguments[0] as ConstantExpression)?.Value);
          if (key == null)
          {
            throw new InvalidOperationException("Calls to GetConstant must use a constant string argument");
          }
          if (!constants.TryGetValue(key, out object value))
          {
            throw new InvalidOperationException($"Call to GetConstant with unknown key \"{key}\"");
          }

          if (value is DateTimeOffset)
          {
            return base.Visit(GetDateTimeOffsetConstant((DateTimeOffset)value));
          }
          else if (value is DateTime)
          {
            return base.Visit(GetDateTimeConstant((DateTime)value));
          }

          return base.VisitConstant(Expression.Constant(value, node.Type));
        }
        else if (mi.Name == "GetProjected")
        {
          var typeArgs = mi.GetGenericArguments();

          LambdaExpression connectorLambda = ((UnaryExpression)node.Arguments[0]).Operand as LambdaExpression;
          var connectorRoot = connectorLambda.Body;
          while (!(connectorRoot is ParameterExpression))
          {
            if (!(connectorRoot is MemberExpression propertyExpr)) throw new InvalidOperationException("Connection expressions must be simple property getters");
            connectorRoot = propertyExpr.Expression;
          }

          if (!projections.TryGetValue($"{typeArgs[0].Name}-{typeArgs[1].Name}", out LambdaExpression templateExpr))
          {
            throw new InvalidOperationException($"No projection found to go from {typeArgs[0].Name} to {typeArgs[1].Name}");
          }

          var connector = new RerootVisitor((ParameterExpression)connectorRoot, root.Peek()).Visit(connectorLambda.Body);
          var modifiedTemplate = new RerootVisitor(templateExpr.Parameters[0], connector).Visit(templateExpr.Body);
          Expression newBranch = base.Visit(modifiedTemplate);

          return base.Visit(newBranch);
        }
        else if (mi.Name == "GetProjection")
        {
          var typeArgs = mi.GetGenericArguments();
          if (!projections.TryGetValue($"{typeArgs[0].Name}-{typeArgs[1].Name}", out LambdaExpression templateExpr))
          {
            throw new InvalidOperationException($"No projection found to go from {typeArgs[0].Name} to {typeArgs[1].Name}");
          }

          root.Push(templateExpr.Parameters[0]);
          var newBranch = base.Visit(templateExpr);
          root.Pop();
          return newBranch;
        }
      }
      return base.VisitMethodCall(node);
    }

    protected override Expression VisitMemberInit(MemberInitExpression node)
    {
      var filteredBindings = new List<MemberBinding>();

      var resourceType = naming.GetPropertyName(node.Type.Name, false);

      var idProperty = node.Type.GetProperties().Where(f => f.GetCustomAttribute<KeyAttribute>() != null || f.Name == "Id").ToArray();
      if (idProperty.Length != 1) throw new InvalidOperationException($"Can't identity key property for {node.Type.FullName}. Must have one property named Id or decorated with [Key] attribute");
      var idField = naming.GetPropertyName(idProperty[0].Name, false);

      foreach (var binding in node.Bindings)
      {
        MemberBinding theBinding = binding;
        var name = naming.GetPropertyName(binding.Member.Name, false);
        if (name == idField)
        {
          // ID is always included
        }
        else if (path.Count > 0 && !include.Any(f => f.StartsWith(string.Join("", path.Select(p => p + ".")) + name)))
        {
          continue;
        }
        else if (include.Any(f => f.StartsWith(string.Join("", path.Select(p => p + ".")) + name)) && binding.Member.GetCustomAttribute<ResourcePropertyAttribute>()?.Type == ResourcePropertyType.Relationship)
        {
          // Always keep included relationships
          // If the user does not include this field in a sparse fields list we'll filter it out later
        }
        else if (fields.TryGetValue(resourceType, out string[] sparseFields))
        {
          if (!sparseFields.Contains(name)) continue;
        }
        //else if (binding.Member.GetCustomAttribute<RelatedEntityAttribute>() != null)
        //{
        //  theBinding = Expression.Bind(theBinding.Member, GetRelationshipBinding(((PropertyInfo)theBinding.Member).PropertyType, root.Peek()));
        //}
        filteredBindings.Add(theBinding);
      }

      var newExpr = filteredBindings.Count == 0 ? node : Expression.MemberInit(node.NewExpression, filteredBindings);

      return base.VisitMemberInit(newExpr);
    }

    protected override MemberAssignment VisitMemberAssignment(MemberAssignment node)
    {
      path.Push(naming.GetPropertyName(node.Member.Name, false));
      var result = base.VisitMemberAssignment(node);
      path.Pop();
      return result;
    }

    private Expression GetRelationshipBinding(Type propertyType, Expression relationship)
    {
      /*
.Lambda #Lambda1<System.Func`2[SarData.Database.Data.MembershipRow,SarData.Server.Apis.JsonApi.RelationshipStub]>(SarData.Database.Data.MembershipRow $msRow)
{
    .New SarData.Server.Apis.JsonApi.RelationshipStub(){
        Id = .Call ($msRow.Id).ToString()
    }
}
       */
      var idProperty = propertyType.GetProperties().Where(f => f.GetCustomAttribute<KeyAttribute>() != null || f.Name == "Id").ToArray();
      if (idProperty.Length != 1) throw new InvalidOperationException($"Can't identity key property for {propertyType.FullName}. Must have one property named Id or decorated with [Key] attribute");

      var toStringInfo = propertyType.GetMethod("ToString", new Type[0]);

      var stubIdProperty = typeof(RelationshipStub).GetProperty("Id");

      return Expression.MemberInit(
        Expression.New(typeof(RelationshipStub)),
        new[]
        {
          Expression.Bind(
            stubIdProperty,
            Expression.Call(
              Expression.Property(
                relationship,
                idProperty[0]
              ),
              toStringInfo
            )
          )
        });
    }
  }
}
