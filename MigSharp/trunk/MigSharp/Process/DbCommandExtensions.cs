using System.Data;

namespace MigSharp.Process
{
    internal static class DbCommandExtensions
    {
        public static IDataParameter AddParameter(this IDbCommand dbCommand, string name, DbType dbType, object value)
        {
            IDbDataParameter parameter = dbCommand.CreateParameter();
            parameter.ParameterName = name;
            parameter.DbType = dbType;
            parameter.Value = value;
            dbCommand.Parameters.Add(parameter);
            return parameter;
        }
    }
}