using System;
using System.Data;
using System.Diagnostics;

namespace MigSharp.Versioning
{
    internal class MigrationStep
    {
        private readonly IMigration _migration;

        public MigrationStep(IMigration migration)
        {
            _migration = migration;
        }

        public void Execute(IDbVersion dbVersion, string connectionString)
        {
            using (IDbConnection connection = InitializeConnection(connectionString))
            {
                Debug.Assert(connection.State == ConnectionState.Open);

                using (IDbTransaction transaction = connection.BeginTransaction())
                {
                    Execute(connection);
                    dbVersion.Update(connection, _migration);
                }
            }
        }

        private void Execute(IDbConnection connection)
        {
            Debug.Assert(connection.State == ConnectionState.Open);

            throw new NotImplementedException();
        }

        private IDbConnection InitializeConnection(string connectionString)
        {
            throw new NotImplementedException();
        }
    }
}