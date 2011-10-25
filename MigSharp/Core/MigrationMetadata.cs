namespace MigSharp.Core
{
    internal class MigrationMetadata : IMigrationMetadata
    {
        private readonly long _timestamp;
        private readonly string _moduleName;
        private readonly string _tag;

        /// <summary>
        /// Gets the timestamp of the migration.
        /// </summary>
        public long Timestamp { get { return _timestamp; } }

        /// <summary>
        /// Gets the module name of the migration.
        /// </summary>
        public string ModuleName { get { return _moduleName; } }

        /// <summary>
        /// Gets the associated tag of the migration.
        /// </summary>
        public string Tag { get { return _tag; } }

        public MigrationMetadata(long timestamp, string moduleName, string tag)
        {
            _timestamp = timestamp;
            _moduleName = moduleName;
            _tag = tag;
        }
    }
}