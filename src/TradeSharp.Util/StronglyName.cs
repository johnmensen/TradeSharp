using System;
using System.Linq.Expressions;

namespace TradeSharp.Util
{
    public static class StronglyName
    {
        public static string Property<TModel, TProperty>(this TModel test, Expression<Func<TModel, TProperty>> property)
        {
            var memberExpression = property.Body as MemberExpression;
            return memberExpression.Member.Name;
        }

        public static string GetMethodName<T>(Expression<Action<T>> method)
        {
            return ((MethodCallExpression)method.Body).Method.Name;
        }
    }
}
