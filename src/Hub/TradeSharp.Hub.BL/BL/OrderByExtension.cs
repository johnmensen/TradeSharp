using System.Linq;
using System.Linq.Expressions;

namespace TradeSharp.Hub.BL.BL
{
    public static class OrderByExtension
    {
        public static IQueryable<T> OrderByField<T>(this IQueryable<T> q, string sortField, bool @ascending, 
            string primKeyField = "")
        {
            var param = Expression.Parameter(typeof(T), "p");
            var prop = Expression.Property(param, sortField);
            if (!string.IsNullOrEmpty(primKeyField))
            {
                var subProp = Expression.Property(prop, primKeyField);
                prop = subProp;
            }

            var exp = Expression.Lambda(prop, param);
            
            var method = @ascending ? "OrderBy" : "OrderByDescending";
            var types = new [] { q.ElementType, exp.Body.Type };
            var mce = Expression.Call(typeof(Queryable), method, types, q.Expression, exp);
            return q.Provider.CreateQuery<T>(mce);
        }
    }
}
