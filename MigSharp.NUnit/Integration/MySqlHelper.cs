using System.Data;

namespace MigSharp.NUnit.Integration
{
    internal static class MySqlHelper
    {
        public static void ActivateStrictMode(IDatabase db)
        {
            // MySQL does not throw certain errors unless strict mode is enabled
            if (db.Context.ProviderMetadata.Name == ProviderNames.MySql)
            {
                db.Execute(context =>
                {
                    IDbCommand command = context.Connection.CreateCommand();
                    command.Transaction = context.Transaction;
                    command.CommandText = "SET SQL_MODE = 'ANSI_QUOTES,STRICT_ALL_TABLES'";
                    command.ExecuteNonQuery();
                });
            }
        }
    }
}