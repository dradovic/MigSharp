using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;

using MigSharp.Core;

namespace MigSharp.Process
{
    using Providers;

    internal class History
    {
        private readonly string _tableName;
        private readonly IProviderMetadata _providerMetadata;
        private readonly IProvider _provider;

        private readonly List<IMigrationMetadata> _actualEntries = new List<IMigrationMetadata>();
        private readonly List<IMigrationMetadata> _entriesToDelete = new List<IMigrationMetadata>();
        private readonly List<IMigrationMetadata> _entriesToInsert = new List<IMigrationMetadata>();

        public History(string tableName, IProviderMetadata providerMetadata, IProvider provider)
        {
            _tableName = tableName;
            _providerMetadata = providerMetadata;
            _provider = provider;
        }

        public IEnumerable<IMigrationMetadata> GetMigrations()
        {
            return _actualEntries;
        }

        public void Insert(long timestamp, string moduleName, string tag)
        {
            _entriesToInsert.Add(new MigrationMetadata(timestamp, moduleName, tag));
        }

        public void Delete(long timestamp, string moduleName)
        {
            _entriesToDelete.Add(new MigrationMetadata(timestamp, moduleName, string.Empty)); // tag is not relevant
        }

        public void Load(IDbConnection connection, IDbTransaction transaction)
        {
            IDbCommand command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = string.Format(CultureInfo.InvariantCulture, "SELECT \"{0}\", \"{1}\", \"{2}\" FROM \"{3}\"",
                BootstrapMigration.TimestampColumnName,
                BootstrapMigration.ModuleColumnName,
                BootstrapMigration.TagColumnName,
                _tableName);
            Log.Verbose(LogCategory.Sql, command.CommandText);

            // Teradata provider does not behave as expected: when using CommandBehavior.SingleResult, reader.Read() will return true even if there are no rows
            using (IDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    long timestamp = Convert.ToInt64(reader[0], CultureInfo.InvariantCulture); // Oracle throws invalid cast exception if using reader[0].ToInt64(0)
                    string moduleName = reader.GetString(1);
                    string tag = reader.IsDBNull(2) ? null : reader.GetString(2);
                    LoadEntry(timestamp, moduleName, tag);
                }
            }
        }

        internal void LoadEntry(long timestamp, string moduleName, string tag)
        {
            var entry = new MigrationMetadata(timestamp, moduleName, tag);
            _actualEntries.Add(entry);
        }

        public void Store(IDbConnection connection, IDbTransaction transaction, IDbCommandExecutor executor)
        {
            Debug.Assert(connection.State == ConnectionState.Open);

            DateTime start = DateTime.Now;
            foreach (IMigrationMetadata entry in _entriesToDelete)
            {
                IDbCommand command = connection.CreateCommand();
                command.Transaction = transaction;
                string moduleName = entry.ModuleName;
                IDataParameter moduleNameParameter = command.AddParameter("@ModuleName", DbType.String, moduleName);
                long timestamp = entry.Timestamp;
                command.CommandText = string.Format(CultureInfo.InvariantCulture, "DELETE FROM {0} WHERE {1} = {2} AND {3} = {4}",
                    _provider.Escape(_tableName),
                    _provider.Escape(BootstrapMigration.TimestampColumnName),
                    timestamp.ToString(CultureInfo.InvariantCulture),
                    _provider.Escape(BootstrapMigration.ModuleColumnName),
                    _providerMetadata.GetParameterSpecifier(moduleNameParameter));
                // note: we do not provide the timestamp as a parameter as the OracleOdbcProvider has an issue with it
                executor.ExecuteNonQuery(command);
                Predicate<IMigrationMetadata> match = m => m.Timestamp == timestamp && m.ModuleName == moduleName;
                Debug.Assert(_actualEntries.FindAll(match).Count == 1, "There should be one existing entry if it is deleted.");
                _actualEntries.RemoveAll(match);
            }
            _entriesToDelete.Clear();
            foreach (IMigrationMetadata entry in _entriesToInsert)
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
                _actualEntries.Add(entry);
            }
            _entriesToInsert.Clear();
            Log.Verbose(LogCategory.Performance, "Version update took {0}s", (DateTime.Now - start).TotalSeconds);
        }
    }
}