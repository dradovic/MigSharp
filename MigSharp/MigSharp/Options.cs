namespace MigSharp
{
    /// <summary>
    /// Use this class to configure MigSharp.
    /// </summary>
    public static class Options
    {
        static Options()
        {
            // set default(s)
            VersioningTableName = "MigSharp";
        }

        /// <summary>
        /// Gets or sets the table name of the versioning table.
        /// </summary>
        public static string VersioningTableName { get; set; }
    }
}