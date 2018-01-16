namespace MigSharp.Process
{
    internal class MigrationExportMetadata
    {
        /// <summary>
        /// Gets the associated tag of the migration.
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// Gets the module name of the migration.
        /// </summary>
        public string ModuleName { get; set; }

        /// <summary>
        /// Indicates if the module name should be used as the default schema if no other schema name was specified for a command. SQL Server only.
        /// </summary>
        public bool UseModuleNameAsDefaultSchema { get; set; }
    }
}