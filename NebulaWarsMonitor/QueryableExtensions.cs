using System;
using System.Linq;
using System.Linq.Expressions;

namespace NebulaWarsMonitor
{
    // From: https://stackoverflow.com/questions/26550930/apply-orderby-with-dbset
    public static class QueryableExtensions
    {
        public static IQueryable<T> OrderBy<T>(this IQueryable<T> source, string orderBy, Type type = null)
        {
            type = type ?? typeof(T);
            return source.GetOrderByQuery(orderBy, "OrderBy", type);
        }

        public static IQueryable<T> OrderByDescending<T>(this IQueryable<T> source, string orderBy, Type type = null)
        {
            type = type ?? typeof(T);
            return source.GetOrderByQuery(orderBy, "OrderByDescending", type);
        }

        private static IQueryable<T> GetOrderByQuery<T>(this IQueryable<T> source, string orderBy, string methodName, Type type = null)
        {
            var sourceType = type ?? typeof(T);
            var property = sourceType.GetProperty(orderBy);
            var parameterExpression = Expression.Parameter(sourceType, "x");
            var getPropertyExpression = Expression.MakeMemberAccess(parameterExpression, property);
            var orderByExpression = Expression.Lambda(getPropertyExpression, parameterExpression);
            var resultExpression = Expression.Call(typeof(Queryable), methodName,
                new[] { sourceType, property.PropertyType }, source.Expression,
                orderByExpression);

            return source.Provider.CreateQuery<T>(resultExpression);
        }
    }
}
