using System.Collections.Generic;
using System.Linq;

using MigSharp.Core;
using MigSharp.Core.Commands;
using MigSharp.Core.Entities;
using MigSharp.Providers;

namespace MigSharp.Process
{
    internal class MigrationReport : IMigrationReport
    {
        private readonly string _migrationName;
        private readonly string _error;
        private readonly Dictionary<DataType, bool> _dataTypes = new Dictionary<DataType, bool>();
        private readonly string _longestName;
        private readonly List<string> _methods = new List<string>();

        public string MigrationName { get { return _migrationName; } }

        public string Error { get { return _error; } }

        public IEnumerable<DataType> DataTypes { get { return _dataTypes.Keys; } }

        public IEnumerable<DataType> PrimaryKeyDataTypes { get { return _dataTypes.Where(p => p.Value).Select(p => p.Key); } }

        public string LongestName { get { return _longestName; } }

        public IEnumerable<string> Methods { get { return _methods; } }

        public MigrationReport(string migrationName, string error, IRecordedMigration migration)
        {
            _migrationName = migrationName;
            _longestName = migration.NewObjectNames.Longest();
            _error = error;
            foreach (DataType dataType in migration.DataTypes)
            {
                AddUsedDataType(dataType, false);
            }
            foreach (DataType dataType in migration.PrimaryKeyDataTypes)
            {
                AddUsedDataType(dataType, true);
            }
            _methods.AddRange(migration.Methods);
        }

        public static MigrationReport Create(Database database, string migrationName)
        {
            // execute changes in 'database' against a RecordingProvider
            var recordingProvider = new RecordingProvider();
            var translator = new CommandsToSqlTranslator(recordingProvider);
            string error = string.Empty;
            try
            {
                translator.TranslateToSql(database, null).ToList(); // .ToList() is important to effectively trigger the iteration
            }
            catch (InvalidCommandException x)
            {
                error = x.Message;
            }

            // create MigrationReport
            return new MigrationReport(migrationName, error, recordingProvider);
        }

        private void AddUsedDataType(DataType dataType, bool usedAsPrimaryKey)
        {
            if (!_dataTypes.ContainsKey(dataType))
            {
                _dataTypes.Add(dataType, usedAsPrimaryKey);
            }
            else
            {
                _dataTypes[dataType] |= usedAsPrimaryKey;
            }
        }
    }
}