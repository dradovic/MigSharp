using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection;

namespace MigSharp.Process
{
    // inspired by: https://github.com/godsharp/GodSharp.Data.Common.DbProvider
    internal static class DbProviderFactories
    {
        private static readonly Dictionary<string, string> ProviderTypes = new Dictionary<string, string> // invariant name --> type name
        {
            { "System.Data.SqlClient", "System.Data.SqlClient.SqlClientFactory, System.Data.SqlClient" }, 
            { "MySql.Data.MySqlClient", "MySql.Data.MySqlClient.MySqlClientFactory, MySql.Data" },
            { "System.Data.SQLite", "System.Data.SQLite.SQLiteFactory, System.Data.SQLite" },
            { "Oracle.ManagedDataAccess.Client", "Oracle.ManagedDataAccess.Client.OracleClientFactory, Oracle.ManagedDataAccess" },
            { "System.Data.Odbc", "System.Data.Odbc.OdbcFactory, System.Data.Odbc" }
        };

        public static DbProviderFactory GetFactory(string providerInvariantName)
        {
            if (!ProviderTypes.TryGetValue(providerInvariantName, out string factoryTypeName))
            {
                throw new InvalidOperationException($"Unknown provider '{providerInvariantName}'.");
            }
            Type type = Type.GetType(factoryTypeName, true);
            FieldInfo field = type.GetField("Instance", BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public);
            if (field != null && field.FieldType.IsSubclassOf(typeof(DbProviderFactory)))
            {
                object value = field.GetValue(null);
                if (value != null)
                {
                    return (DbProviderFactory)value;
                }
            }
            throw new InvalidOperationException($"Could not get provider factory of '{providerInvariantName}'.");
        }
    }
}