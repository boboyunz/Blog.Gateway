using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Collections.Generic;
namespace Blog.Core.Common.Helper
{
    public static class ExpressionExtensions
    {
        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
        {
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T, bool>>(Expression.Or(expr1.Body, invokedExpr), expr1.Parameters);
        }

        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
        {
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T, bool>>(Expression.And(expr1.Body, invokedExpr), expr1.Parameters);
        }

        public static Expression<Func<T, bool>> WhereAnd<T>(this IEnumerable<Expression<Func<T, bool>>> exprs)
        {
            Expression<Func<T, bool>> FinalQuery = t => true;
            foreach (var expr in exprs)
            {
                FinalQuery = FinalQuery.And(expr);
            }
            return FinalQuery;
        }

        public static Expression<Func<T, bool>> WhereOr<T>(this IEnumerable<Expression<Func<T, bool>>> exprs)
        {
            Expression<Func<T, bool>> FinalQuery = t => false;
            foreach (var expr in exprs)
            {
                FinalQuery = FinalQuery.Or(expr);
            }
            return FinalQuery;
        }
    }
}
