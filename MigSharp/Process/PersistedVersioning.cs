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

        public bool IsContained(IMigrationMetadata metadata)
        {
            return _history.Contains(metadata.Timestamp, metadata.ModuleName);
        }

        public void Update(IMigrationMetadata metadata, IDbConnection connection, IDbTransaction transaction, MigrationDirection direction)
        {
            Debug.Assert(!(metadata is BootstrapMetadata));

            if (direction == MigrationDirection.Up)
            {
                _history.Insert(metadata.Timestamp, metadata.ModuleName, metadata.Tag);
            }
            else
            {
                Debug.Assert(direction == MigrationDirection.Down);
                Debug.Assert(_history.Contains(metadata.Timestamp, metadata.ModuleName), "Only migrations that were applied previously are being undone.");
                _history.Delete(metadata.Timestamp, metadata.ModuleName);
            }

            StoreChanges(connection, transaction);
        }

        internal void UpdateToInclude(IEnumerable<IMigrationMetadata> containedMigrations, IDbConnection connection, IDbTransaction transaction)
        {
            foreach (IMigrationMetadata metadata in containedMigrations)
            {
                _history.Insert(metadata.Timestamp, metadata.ModuleName, metadata.Tag);
            }
            StoreChanges(connection, transaction);
        }

        private void StoreChanges(IDbConnection connection, IDbTransaction transaction)
        {
            _history.Store(connection, transaction);
        }
    }
}