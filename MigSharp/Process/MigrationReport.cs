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
        private readonly List<UsedDataType> _dataTypes = new List<UsedDataType>();
        private readonly string _longestName;
        private readonly List<string> _methods = new List<string>();

        public string MigrationName { get { return _migrationName; } }

        public string Error { get { return _error; } }

        public IEnumerable<DataType> DataTypes { get { return _dataTypes.GroupBy(d => d.DataType).Select(g => g.Key); } }

        public IEnumerable<DataType> PrimaryKeyDataTypes { get { return _dataTypes.Where(d => d.UsedAsPrimaryKey).GroupBy(d => d.DataType).Select(g => g.Key); } }

        public IEnumerable<DataType> IdentityDataTypes { get { return _dataTypes.Where(d => d.UsedAsIdentity).GroupBy(d => d.DataType).Select(g => g.Key); } }

        public string LongestName { get { return _longestName; } }

        public IEnumerable<string> Methods { get { return _methods; } }

        public MigrationReport(string migrationName, string error, IRecordedMigration migration)
        {
            _migrationName = migrationName;
            _longestName = migration.NewObjectNames.Longest();
            _error = error;
            _dataTypes.AddRange(migration.DataTypes);
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
    }
}