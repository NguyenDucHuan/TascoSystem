using System.Linq.Expressions;

namespace Tasco.TaskService.API.Configure
{
    public static class ExpressionExtensions
    {
        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> expr1,
            Expression<Func<T, bool>> expr2)
        {
            var parameter = Expression.Parameter(typeof(T));

            var visitor = new ReplacingExpressionVisitor();
            visitor.Add(expr1.Parameters[0], parameter);
            visitor.Add(expr2.Parameters[0], parameter);

            var combined = visitor.Visit(Expression.AndAlso(expr1.Body, expr2.Body));

            return Expression.Lambda<Func<T, bool>>(combined, parameter);
        }

        private class ReplacingExpressionVisitor : ExpressionVisitor
        {
            private readonly Dictionary<Expression, Expression> _replacements;

            public ReplacingExpressionVisitor()
            {
                _replacements = new Dictionary<Expression, Expression>();
            }

            public void Add(Expression original, Expression replacement)
            {
                _replacements[original] = replacement;
            }

            public override Expression Visit(Expression? node)
            {
                if (node != null && _replacements.TryGetValue(node, out var replacement))
                {
                    return replacement;
                }

                return base.Visit(node) ?? node;
            }
        }
    }
}