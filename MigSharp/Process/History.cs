using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;

using MigSharp.Core;

namespace MigSharp.Process
{
    internal class History
    {
        private readonly string _tableName;
        private readonly IProviderMetadata _providerMetadata;

        private readonly Dictionary<long, List<string>> _actualEntries = new Dictionary<long, List<string>>(); // timestamp -> moduleNames
        private readonly List<HistoryEntryId> _entriesToDelete = new List<HistoryEntryId>();
        private readonly List<HistoryEntry> _entriesToInsert = new List<HistoryEntry>();

        public History(string tableName, IProviderMetadata providerMetadata)
        {
            _tableName = tableName;
            _providerMetadata = providerMetadata;
        }

        public bool Contains(long timestamp, string moduleName)
        {
            List<string> moduleNames;
            if (_actualEntries.TryGetValue(timestamp, out moduleNames))
            {
                return moduleNames.Contains(moduleName);
            }
            return false;
        }

        public void Insert(long timestamp, string moduleName, string tag)
        {
            _entriesToInsert.Add(new HistoryEntry(timestamp, moduleName, tag));
        }

        public void Delete(long timestamp, string moduleName)
        {
            _entriesToDelete.Add(new HistoryEntryId(timestamp, moduleName));
        }

        public void Load(IDbConnection connection, IDbTransaction transaction)
        {
            IDbCommand command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = string.Format(CultureInfo.InvariantCulture, "SELECT \"{0}\", \"{1}\" FROM \"{2}\"",
                BootstrapMigration.TimestampColumnName,
                BootstrapMigration.ModuleColumnName,
                _tableName);
            Log.Verbose(LogCategory.Sql, command.CommandText);

            // Teradata provider does not behave as expected: when using CommandBehavior.SingleResult, reader.Read() will return true even if there are no rows
            using (IDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    long timestamp = Convert.ToInt64(reader[0], CultureInfo.InvariantCulture); // Oracle throws invalid cast exception if using reader[0].ToInt64(0)
                    string moduleName = reader.GetString(1);
                    LoadEntry(timestamp, moduleName);
                }
            }
        }

        internal void LoadEntry(long timestamp, string moduleName)
        {
            HistoryEntryId entryId = new HistoryEntryId(timestamp, moduleName);
            AddToActualEntries(entryId);
        }

        public void Store(IDbConnection connection, IDbTransaction transaction, IDbCommandExecutor executor)
        {
            Debug.Assert(connection.State == ConnectionState.Open);

            DateTime start = DateTime.Now;
            foreach (HistoryEntryId entryId in _entriesToDelete)
            {
                IDbCommand command = connection.CreateCommand();
                command.Transaction = transaction;
                IDataParameter moduleNameParameter = command.AddParameter("@ModuleName", DbType.String, entryId.ModuleName);
                command.CommandText = string.Format(CultureInfo.InvariantCulture, "DELETE FROM \"{0}\" WHERE \"{1}\" = {2} AND \"{3}\" = {4}",
                    _tableName,
                    BootstrapMigration.TimestampColumnName,
                    entryId.Timestamp.ToString(CultureInfo.InvariantCulture),
                    BootstrapMigration.ModuleColumnName,
                    _providerMetadata.GetParameterSpecifier(moduleNameParameter));
                // note: we do not provide the timestamp as a parameter as the OracleOdbcProvider has an issue with it
                executor.ExecuteNonQuery(command);
                RemoveFromActualEntries(entryId);
            }
            _entriesToDelete.Clear();
            foreach (HistoryEntry entry in _entriesToInsert)
            {
                IDbCommand command = connection.CreateCommand();
                command.Transaction = transaction;
                IDataParameter moduleNameParameter = command.AddParameter("@ModuleName", DbType.String, entry.ModuleName);
                IDataParameter tagParameter = command.AddParameter("@Tag", DbType.String, !string.IsNullOrEmpty(entry.Tag) ? (object)entry.Tag : DBNull.Value);
                command.CommandText = string.Format(CultureInfo.InvariantCulture, @"INSERT INTO ""{0}"" (""{1}"", ""{2}"", ""{3}"") VALUES ({4}, {5}, {6})",
                    _tableName,
                    BootstrapMigration.TimestampColumnName,
                    BootstrapMigration.ModuleColumnName,
                    BootstrapMigration.TagColumnName,
                    entry.Timestamp.ToString(CultureInfo.InvariantCulture),
                    _providerMetadata.GetParameterSpecifier(moduleNameParameter),
                    _providerMetadata.GetParameterSpecifier(tagParameter));
                // note: we do not provide the timestamp as a parameter as the OracleOdbcProvider has an issue with it
                executor.ExecuteNonQuery(command);
                AddToActualEntries(entry);
            }
            _entriesToInsert.Clear();
            Log.Verbose(LogCategory.Performance, "Version update took {0}s", (DateTime.Now - start).TotalSeconds);
        }

        private void RemoveFromActualEntries(HistoryEntryId entryId)
        {
            List<string> moduleNames;
            if (_actualEntries.TryGetValue(entryId.Timestamp, out moduleNames))
            {
                moduleNames.Remove(entryId.ModuleName);
                if (moduleNames.Count == 0)
                {
                    _actualEntries.Remove(entryId.Timestamp);
                }
            }
        }

        private void AddToActualEntries(HistoryEntryId entryId)
        {
            List<string> moduleNames;
            if (!_actualEntries.TryGetValue(entryId.Timestamp, out moduleNames))
            {
                moduleNames = new List<string>();
                _actualEntries.Add(entryId.Timestamp, moduleNames);
            }
            moduleNames.Add(entryId.ModuleName);
        }

        private class HistoryEntryId
        {
            private readonly long _timestamp;
            private readonly string _moduleName;

            public long Timestamp { get { return _timestamp; } }
            public string ModuleName { get { return _moduleName; } }

            public HistoryEntryId(long timestamp, string moduleName)
            {
                _timestamp = timestamp;
                _moduleName = moduleName;
            }
        }

        private class HistoryEntry : HistoryEntryId
        {
            private readonly string _tag;

            public string Tag { get { return _tag; } }

            public HistoryEntry(long timestamp, string moduleName, string tag) : base(timestamp, moduleName)
            {
                _tag = tag;
            }
        }
    }
}