using System;
using System.Globalization;
using ServiceStack.OrmLite.SqlServer;
using ServiceStack.Text;

namespace ConsoleApplication4
{
    public class FixedSqlServerOrmLiteDialectProvider : SqlServerOrmLiteDialectProvider
    {
        public static FixedSqlServerOrmLiteDialectProvider Instance = new FixedSqlServerOrmLiteDialectProvider();
        public override string GetQuotedValue(object value, Type fieldType)
        {
            if (value == null) return "NULL";

            if (!fieldType.UnderlyingSystemType.IsValueType && fieldType != typeof(string))
            {
                if (TypeSerializer.CanCreateFromString(fieldType))
                {
                    return "'" + EscapeParam(TypeSerializer.SerializeToString(value)) + "'";
                }

                throw new NotSupportedException(
                    string.Format("Property of type: {0} is not supported", fieldType.FullName));
            }

            if (fieldType == typeof(float))
                return ((float)value).ToString(CultureInfo.InvariantCulture);

            if (fieldType == typeof(double))
                return ((double)value).ToString(CultureInfo.InvariantCulture);

            if (fieldType == typeof(decimal))
                return ((decimal)value).ToString(CultureInfo.InvariantCulture);

            return ShouldQuoteValue(fieldType)
                    ? "N'" + EscapeParam(value) + "'"
                    : value.ToString();
        }
    }
}