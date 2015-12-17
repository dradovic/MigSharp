using System;
using System.Diagnostics;
using System.IO;
using JetBrains.Annotations;
using MigSharp.Core;
using MigSharp.Process;

namespace MigSharp
{
    /// <summary>
    /// Use this class to configure the behaviour of the <see cref="Migrator"/>.
    /// </summary>
    public class MigrationOptions : DbAltererOptions
    {
        /// <summary>
        /// The default table name for the table that track the history of the migrations.
        /// </summary>
        public const string DefaultVersioningTableName = "MigSharp";

        private string _versioningTableName = DefaultVersioningTableName;
        private string _versioningTableSchema;

        /// <summary>
        /// Gets or sets the table name of the versioning table.
        /// </summary>
        [NotNull]
        public string VersioningTableName
        {
            get { return _versioningTableName; }
            set
            {
                if (string.IsNullOrEmpty(value)) throw new ArgumentException("The versioning table name cannot be empty.");
                _versioningTableName = value;
            }
        }

        /// <summary>
        /// Gets or sets the schema name of the versioning table. SQL Server only.
        /// </summary>
        public string VersioningTableSchema { get { return _versioningTableSchema; } set { _versioningTableSchema = value; } }

        internal TableName VersioningTable { get { return new TableName(VersioningTableName, VersioningTableSchema); } }

        private Predicate<string> _moduleSelector = n => true; // select all modules by default

        /// <summary>
        /// Gets or sets a function that selects the module based on its name. Only migrations for this module will be executed.
        /// </summary>
        public Predicate<string> ModuleSelector
        {
            get { return _moduleSelector; }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                _moduleSelector = value;
            }
        }

        private ScriptingOptions _scriptingOptions = new ScriptingOptions(ScriptingMode.ExecuteOnly, null);
        internal ScriptingOptions ScriptingOptions { get { return _scriptingOptions; } }

        /// <summary>
        /// Initializes an instance of default options.
        /// </summary>
        public MigrationOptions()
        {
        }

        /// <summary>
        /// Initializes options that select migrations for specific module only.
        /// </summary>
        /// <param name="moduleName">The name of the selected module. Only migrations for this module will be executed.</param>
        public MigrationOptions(string moduleName)
        {
            if (string.IsNullOrEmpty(moduleName)) throw new ArgumentException("Empty moduleName.", "moduleName");

            ModuleSelector = n => n == moduleName;
        }

        /// <summary>
        /// Outputs the SQL used for the migrations to external files without affecting the database.
        /// </summary>
        public void OnlyScriptSqlTo(DirectoryInfo targetDirectory) // signature used in Wiki Manual
        {
            _scriptingOptions = new ScriptingOptions(ScriptingMode.ScriptOnly, targetDirectory);
        }

        /// <summary>
        /// Outputs the SQL used for the migrations to external files without affecting the database.
        /// </summary>
        public void OnlyScriptSqlTo(string targetDirectory) // signature used in Wiki Manual
        {
            OnlyScriptSqlTo(new DirectoryInfo(targetDirectory));
        }

        /// <summary>
        /// Outputs the SQL used for the migrations to external files while migrating the database.
        /// </summary>
        public void ExecuteAndScriptSqlTo(DirectoryInfo targetDirectory) // signature used in Wiki Manual
        {
            _scriptingOptions = new ScriptingOptions(ScriptingMode.ScriptAndExecute, targetDirectory);
        }

        /// <summary>
        /// Outputs the SQL used for the migrations to external files while migrating the database.
        /// </summary>
        public void ExecuteAndScriptSqlTo(string targetDirectory) // signature used in Wiki Manual
        {
            ExecuteAndScriptSqlTo(new DirectoryInfo(targetDirectory));
        }

        #region Static Options

        ///<summary>
        /// Sets the level of general information being traced.
        ///</summary>
        [Obsolete("Use static Options class instead.")]
        public static void SetGeneralTraceLevel(SourceLevels sourceLevels)
        {
            Options.SetGeneralTraceLevel(sourceLevels);
        }

        ///<summary>
        /// Sets the level of SQL information being traced.
        ///</summary>
        [Obsolete("Use static Options class instead.")]
        public static void SetSqlTraceLevel(SourceLevels sourceLevels) // signature used in a Wiki example
        {
            Options.SetSqlTraceLevel(sourceLevels);
        }

        ///<summary>
        /// Sets the level of performance information being traced.
        ///</summary>
        [Obsolete("Use static Options class instead.")]
        public static void SetPerformanceTraceLevel(SourceLevels sourceLevels)
        {
            Options.SetPerformanceTraceLevel(sourceLevels);
        }

        #endregion
    }
}