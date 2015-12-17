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

        public void Update(IScheduledMigrationMetadata metadata, IDbConnection connection, IDbTransaction transaction, IDbCommandExecutor commandExecutor)
        {
            if (metadata.Direction == MigrationDirection.Up)
            {
                _history.Insert(metadata.Timestamp, metadata.ModuleName, metadata.Tag);
            }
            else
            {
                Debug.Assert(metadata.Direction == MigrationDirection.Down);
                _history.Delete(metadata.Timestamp, metadata.ModuleName);
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