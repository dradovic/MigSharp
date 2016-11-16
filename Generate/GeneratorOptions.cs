using System.Collections.Generic;

namespace MigSharp.Generate
{
    public class GeneratorOptions
    {
        private readonly List<string> _includedSchemas = new List<string>();
        private readonly List<string> _excludedTables = new List<string>();
        private string _versioningTableName = MigrationOptions.DefaultVersioningTableName;
        private string _moduleName = MigrationExportAttribute.DefaultModuleName;
        private string _versioningTableSchema = "dbo";

        public string Namespace { get; set; }

        public string ModuleName { get { return _moduleName; } set { _moduleName = value; } }

        public string VersioningTableName { get { return _versioningTableName; } set { _versioningTableName = value; } }

        public string VersioningTableSchema { get { return _versioningTableSchema; } set { _versioningTableSchema = value; } }

        public ICollection<string> IncludedSchemas { get { return _includedSchemas; } }

        public ICollection<string> ExcludedTables { get { return _excludedTables; } }

        public bool IncludeData { get; set; }

        internal bool IsSchemaIncluded(string schema)
        {
            return _includedSchemas.Count == 0 || _includedSchemas.Contains(schema);
        }

        internal bool IsTableIncluded(string tableName)
        {
            return _excludedTables.Count == 0 || !_excludedTables.Contains(tableName);
        }
    }
}