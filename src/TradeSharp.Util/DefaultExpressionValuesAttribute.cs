using System;

namespace TradeSharp.Util
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DefaultExpressionValuesAttribute : Attribute
    {
        /// <summary>
        /// оператор для сравнения (проверки критерия) по-умолчанию
        /// </summary>
        public ExpressionOperator Operator { get; set; }

        /// <summary>
        /// значение для сравнения по-умолчанию
        /// </summary>
        public double Value { get; set; }

        public DefaultExpressionValuesAttribute(ExpressionOperator oper, double value)
        {
            Operator = oper;
            Value = value;
        }
    }
}
