using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Open.ExpressionVisitor
{
    internal static class ExpressionHelper
    {
        /// <summary>
        /// 获取Expression NodeType表示的操作符
        /// </summary>
        /// <param name="nodeType"></param>
        /// <returns></returns>
        public static string GetOperator(ExpressionType nodeType)
        {
            switch (nodeType)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    return " AND ";
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    return " OR ";
                case ExpressionType.Equal:
                    return " = ";
                case ExpressionType.NotEqual:
                    return " <> ";
                case ExpressionType.GreaterThan:
                    return " > ";
                case ExpressionType.GreaterThanOrEqual:
                    return " >= ";
                case ExpressionType.LessThan:
                    return " < ";
                case ExpressionType.LessThanOrEqual:
                    return " <= ";
            }
            throw new NotSupportedException($"不支持的NodeType: {nodeType}");
        }

    }
}
