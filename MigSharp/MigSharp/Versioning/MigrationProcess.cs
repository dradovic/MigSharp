using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;

namespace MigSharp.Versioning
{
    internal class MigrationProcess
    {
        private readonly string _connectionString;
        private readonly IEnumerable<Lazy<IMigration, IMigrationMetaData>> _migrations;

        public MigrationProcess(string connectionString, IEnumerable<Lazy<IMigration, IMigrationMetaData>> migrations)
        {
            Debug.Assert(migrations.Count() > 0);

            _connectionString = connectionString;
            _migrations = migrations;
        }

        public void Do(IDbVersion dbVersion)
        {
            using (DbConnection connection = InitializeConnection())
            {
                Debug.Assert(connection.State == ConnectionState.Open);
                var applicableMigrations = from m in _migrations
                                           where !dbVersion.Includes(m.Metadata)
                                           orderby m.Metadata.Timestamp
                                           select m.Value;
                foreach (IMigration migration in applicableMigrations)
                {
                    using (DbTransaction transaction = connection.BeginTransaction())
                    {
                        DoMigration(connection, migration);
                        dbVersion.Update(connection, migration);
                    }
                }
            }
        }

        private void DoMigration(DbConnection connection, IMigration migration)
        {
            throw new NotImplementedException();
        }

        private DbConnection InitializeConnection()
        {
            throw new NotImplementedException();
        }
    }
}