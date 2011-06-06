using System;
using System.Data;
using System.Globalization;

namespace MigSharp.Providers
{
    internal static class SqlScriptingHelper
    {
        public static string ToSql(object value, DbType dbType)
        {
            if (value == null) throw new ArgumentNullException("value");

            if (DBNull.Value.Equals(value))
            {
                return "NULL";
            }

            switch (dbType)
            {
                case DbType.AnsiString:
                case DbType.AnsiStringFixedLength:
                case DbType.String:
                case DbType.StringFixedLength:
                    return "'" + Convert.ToString(value, CultureInfo.InvariantCulture).Replace("'", "''") + "'";
                case DbType.Byte:
                    return Convert.ToByte(value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
                case DbType.Boolean:
                    return Convert.ToBoolean(value, CultureInfo.InvariantCulture) ? "1" : "0";
                case DbType.Decimal:
                case DbType.VarNumeric:
                    return Convert.ToDecimal(value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
                case DbType.Date:
                    return "'" + ((DateTime)value).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + "'"; // ISO 8601                    
                case DbType.DateTime:
                    return "'" + ((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) + "'"; // ISO 8601                    
                case DbType.Double:
                    return Convert.ToDouble(value, CultureInfo.InvariantCulture).ToString("r", CultureInfo.InvariantCulture);
                case DbType.Int16:
                    return Convert.ToInt16(value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
                case DbType.Int32:
                    return Convert.ToInt32(value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
                case DbType.Int64:
                    return Convert.ToInt64(value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
                case DbType.SByte:
                    return Convert.ToSByte(value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
                case DbType.Single:
                    return Convert.ToSingle(value, CultureInfo.InvariantCulture).ToString("r", CultureInfo.InvariantCulture);
                case DbType.UInt16:
                    return Convert.ToUInt16(value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
                case DbType.UInt32:
                    return Convert.ToUInt32(value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
                case DbType.UInt64:
                    return Convert.ToUInt64(value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
                default:
                    throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Values of type {0} cannot be scripted.", dbType));
            }
        }
    }
}