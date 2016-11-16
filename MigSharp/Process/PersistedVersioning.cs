using System.Collections.Generic;
using System.Data;
using System.Diagnostics;

namespace MigSharp.Process
{
    internal class PersistedVersioning : IVersioning
    {
        private readonly History _history;

        internal PersistedVersioning(History history)
        {
            _history = history;
        }

        public IEnumerable<IMigrationMetadata> ExecutedMigrations { get { return _history.GetMigrations(); } }

        public void Update(IMigrationStepMetadata metadata, IDbConnection connection, IDbTransaction transaction, IDbCommandExecutor commandExecutor)
        {
            foreach (IMigrationMetadata migrationMetadata in metadata.Migrations)
            {
                Debug.Assert(migrationMetadata.ModuleName == metadata.ModuleName, "The migration module name must correspond to the module name of the scheduled migration step.");
                if (metadata.Direction == MigrationDirection.Up)
                {
                    _history.Insert(migrationMetadata.Timestamp, metadata.ModuleName, migrationMetadata.Tag);
                }
                else
                {
                    Debug.Assert(metadata.Direction == MigrationDirection.Down);
                    _history.Delete(migrationMetadata.Timestamp, metadata.ModuleName);
                }
            }

            StoreChanges(connection, transaction, commandExecutor);
        }

        internal void UpdateToInclude(IEnumerable<IMigrationMetadata> containedMigrations, IDbConnection connection, IDbTransaction transaction, IDbCommandExecutor executor)
        {
            foreach (IMigrationMetadata metadata in containedMigrations)
            {
                _history.Insert(metadata.Timestamp, metadata.ModuleName, metadata.Tag);
            }
            StoreChanges(connection, transaction, executor);
        }

        private void StoreChanges(IDbConnection connection, IDbTransaction transaction, IDbCommandExecutor executor)
        {
            _history.Store(connection, transaction, executor);
        }
    }
}