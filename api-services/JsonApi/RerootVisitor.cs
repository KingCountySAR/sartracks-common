using System.Linq.Expressions;

class RerootVisitor : ExpressionVisitor
{
  private readonly ParameterExpression oldRoot;
  private readonly Expression newRoot;

  public RerootVisitor(ParameterExpression oldRoot, Expression newRoot)
  {
    this.oldRoot = oldRoot;
    this.newRoot = newRoot;
  }

  protected override Expression VisitParameter(ParameterExpression node)
  {
    return node == oldRoot ?  newRoot : base.VisitParameter(node);
  }
}
