using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Open.ExpressionVisitor
{
    /// <summary>
    /// Sql语句
    /// </summary>
    public class ExpressionSqlWriter : ExpressionVisitor
    {
        StringBuilder m_sb;
        ExpressionType m_nodeType;
        Expression m_left;

        public ExpressionSqlWriter()
        {
            m_sb = new StringBuilder();
            m_nodeType = 0;
            m_left = null;
        }

        public string Translate(Expression expression)
        {
            expression = Evaluator.PartialEval(expression);
            base.Visit(expression);
            return m_sb.ToString();
        }

        protected override Expression VisitBinary(BinaryExpression b)
        {
            m_sb.Append("(");

            Expression left = b.Left;

            base.Visit(left);

            m_sb.Append(ExpressionHelper.GetOperator(b.NodeType));

            m_nodeType = b.NodeType;

            Expression right = b.Right;

            base.Visit(right);

            m_sb.Append(")");

            m_nodeType = 0;

            return b;
        }

        protected override Expression VisitMemberAccess(MemberExpression m)
        {
            if (m.Member.DeclaringType == typeof(string))
            {
                switch (m.Member.Name)
                {
                    case "Length":
                        m_sb.Append("LEN(");
                        base.Visit(m.Expression);
                        m_sb.Append(")");
                        break;
                }
            }
            else if (m.Member.DeclaringType == typeof(DateTime) || m.Member.DeclaringType == typeof(DateTimeOffset))
            {
                switch (m.Member.Name)
                {
                    case "Year":
                        m_sb.Append("YEAR(");
                        base.Visit(m.Expression);
                        m_sb.Append(")");
                        break;
                    case "Month":
                        m_sb.Append("MONTH(");
                        base.Visit(m.Expression);
                        m_sb.Append(")");
                        break;
                    case "Day":
                        m_sb.Append("DAY(");
                        base.Visit(m.Expression);
                        m_sb.Append(")");
                        break;
                    case "Hour":
                        m_sb.Append("DATEPART(HOUR, ");
                        base.Visit(m.Expression);
                        m_sb.Append(")");
                        break;
                    case "Minute":
                        m_sb.Append("DATEPART(MINUTE, ");
                        base.Visit(m.Expression);
                        m_sb.Append(")");
                        break;
                    case "Second":
                        m_sb.Append("DATEPART(SECOND, ");
                        base.Visit(m.Expression);
                        m_sb.Append(")");
                        break;
                    case "Millisecond":
                        m_sb.Append("DATEPART(MILLISECOND, ");
                        base.Visit(m.Expression);
                        m_sb.Append(")");
                        break;
                    case "DayOfWeek":
                        m_sb.Append("(DATEPART(WEEKDAY, ");
                        base.Visit(m.Expression);
                        m_sb.Append(") - 1)");
                        break;
                    case "DayOfYear":
                        m_sb.Append("(DATEPART(DAYOFYEAR, ");
                        base.Visit(m.Expression);
                        m_sb.Append(") - 1)");
                        break;
                }
            }
            else
            {
                m_sb.AppendFormat("[{0}]", m.Member.Name);
                m_left = m;
            }
            return m;
        }

        protected override Expression VisitConstant(ConstantExpression c)
        {
            object value = c.Value;
            if (value == null)
            {
                if (m_nodeType == ExpressionType.Equal)
                {
                    m_sb.Remove(m_sb.Length - 3, 3);//移除" = "
                    m_sb.Append(" IS NULL ");
                }
                else if (m_nodeType == ExpressionType.NotEqual)
                {
                    m_sb.Remove(m_sb.Length - 3, 3);
                    m_sb.Append(" IS NOT NULL ");
                }
                else
                {
                    m_sb.Append(" NULL ");
                }
            }
            else if (value.GetType() == typeof(Guid)) //guid
            {
                m_sb.AppendFormat("'{0}'", c.Value);
            }
            else if (value is IEnumerable && !(value is string))
            {
                foreach (var item in (IEnumerable)value)
                {
                    Expression constant = Expression.Constant(item);
                    base.Visit(constant);
                    m_sb.Append(",");
                }
                m_sb.Remove(m_sb.Length - 1, 1);//移除末尾,(逗号)
            }
            else if (m_left == null && value is bool)//p=>true
            {
                m_sb.AppendFormat("({0})", ((bool)value) ? "1 = 1" : "1 = 0");
            }
            else
            {
                switch (Type.GetTypeCode(value.GetType()))
                {
                    case TypeCode.Boolean:
                        m_sb.Append(((bool)c.Value) ? "1" : "0");
                        break;
                    case TypeCode.DateTime:
                        m_sb.AppendFormat("'{0}'", ((DateTime)c.Value).ToString("yyyy-MM-dd HH:mm:ss.fff"));
                        break;
                    case TypeCode.String:
                    case TypeCode.Char:
                        m_sb.AppendFormat("'{0}'", c.Value);
                        break;
                    default:
                        m_sb.Append(c.Value);
                        break;
                }
            }

            m_left = null;//访问过右值清空左值引用
            return c;
        }

        protected override Expression VisitUnary(UnaryExpression u)
        {
            switch (u.NodeType)
            {
                case ExpressionType.Not:
                    m_sb.Append(" NOT ");
                    this.Visit(u.Operand);
                    break;
                default:
                    throw new NotSupportedException($"不支持的NodeType: {u.NodeType}");

            }
            return u;
        }

        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            if (m.Method.Name == "Equals")
            {
                base.Visit(m.Object);
                m_sb.Append(" = ");
                base.Visit(m.Arguments[0]);
                return m;
            }
            else if (m.Method.Name == "ToString")
            {
                if (m.Object.Type != typeof(string))
                {
                    m_sb.Append(" CAST(");
                    base.Visit(m.Object);
                    m_sb.Append(" AS NVARCHAR)");
                }
                else
                {
                    base.Visit(m.Object);
                }
                return m;
            }
            else if (m.Method.DeclaringType == typeof(string))
            {
                switch (m.Method.Name)
                {
                    case "StartsWith":
                        m_sb.Append("(");
                        base.Visit(m.Object);
                        m_sb.Append(" LIKE ");
                        base.Visit(m.Arguments[0]);
                        m_sb.Append(" + '%')");
                        return m;
                    case "EndsWith":
                        m_sb.Append("(");
                        base.Visit(m.Object);
                        m_sb.Append(" LIKE '%' + ");
                        base.Visit(m.Arguments[0]);
                        m_sb.Append(")");
                        return m;
                    case "Contains":
                        m_sb.Append("(");
                        base.Visit(m.Object);
                        m_sb.Append(" LIKE '%' + ");
                        base.Visit(m.Arguments[0]);
                        m_sb.Append(" + '%')");
                        return m;
                    case "IsNullOrEmpty":
                        m_sb.Append("(");
                        base.Visit(m.Arguments[0]);
                        m_sb.Append(" IS NULL OR ");
                        base.Visit(m.Arguments[0]);
                        m_sb.Append(" = '' )");
                        return m;
                    case "ToUpper":
                        m_sb.Append("UPPER(");
                        base.Visit(m.Object);
                        m_sb.Append(")");
                        return m;
                    case "ToLower":
                        m_sb.Append("LOWER(");
                        base.Visit(m.Object);
                        m_sb.Append(")");
                        return m;
                    case "Trim":
                        m_sb.Append("LTRIM(RTRIM(");
                        base.Visit(m.Object);
                        m_sb.Append("))");
                        return m;
                    case "TrimStart":
                        m_sb.Append("LTRIM(");
                        base.Visit(m.Object);
                        m_sb.Append(")");
                        return m;
                    case "TrimEnd":
                        m_sb.Append("RTRIM(");
                        base.Visit(m.Object);
                        m_sb.Append(")");
                        return m;
                }
            }
            else if (m.Method.DeclaringType == typeof(Enumerable))//Linq Contains
            {
                switch (m.Method.Name)
                {
                    case "Contains":
                        m_sb.Append("(");
                        base.Visit(m.Arguments[1]);
                        m_sb.Append(" IN (");
                        base.Visit(m.Arguments[0]);
                        m_sb.Append("))");
                        return m;
                }
            }
            else if (typeof(IList).IsAssignableFrom(m.Method.DeclaringType))//List Contains
            {
                switch (m.Method.Name)
                {
                    case "Contains":
                        m_sb.Append("(");
                        base.Visit(m.Arguments[0]);
                        m_sb.Append(" IN (");
                        base.Visit(m.Object);
                        m_sb.Append("))");
                        return m;
                }
            }
            else if (m.Method.DeclaringType == typeof(DateTime))
            {
                switch (m.Method.Name)
                {
                    case "AddYears":
                        m_sb.Append(" DATEADD(YYYY,");
                        base.Visit(m.Arguments[0]);
                        m_sb.Append(",");
                        base.Visit(m.Object);
                        m_sb.Append(")");
                        return m;
                    case "AddMonths":
                        m_sb.Append(" DATEADD(MM,");
                        base.Visit(m.Arguments[0]);
                        m_sb.Append(",");
                        base.Visit(m.Object);
                        m_sb.Append(")");
                        return m;
                    case "AddDays":
                        m_sb.Append(" DATEADD(DD,");
                        base.Visit(m.Arguments[0]);
                        m_sb.Append(",");
                        base.Visit(m.Object);
                        m_sb.Append(")");
                        return m;
                    case "AddHours":
                        m_sb.Append(" DATEADD(HH,");
                        base.Visit(m.Arguments[0]);
                        m_sb.Append(",");
                        base.Visit(m.Object);
                        m_sb.Append(")");
                        return m;
                    case "AddMinutes":
                        m_sb.Append(" DATEADD(MI,");
                        base.Visit(m.Arguments[0]);
                        m_sb.Append(",");
                        base.Visit(m.Object);
                        m_sb.Append(")");
                        return m;
                    case "AddSeconds":
                        m_sb.Append(" DATEADD(SS,");
                        base.Visit(m.Arguments[0]);
                        m_sb.Append(",");
                        base.Visit(m.Object);
                        m_sb.Append(")");
                        return m;
                    case "AddMilliseconds":
                        m_sb.Append(" DATEADD(MS,");
                        base.Visit(m.Arguments[0]);
                        m_sb.Append(",");
                        base.Visit(m.Object);
                        m_sb.Append(")");
                        return m;
                }
            }
            throw new NotSupportedException($"不支持的方法: {m.Method.DeclaringType}.{m.Method.Name}");
        }
    }
}