namespace MigSharp.Core
{
    internal class AggregateMigrationMetadata : IAggregateMigrationMetadata
    {
        private readonly long _timestamp;
        private readonly string _moduleName;

        public string ModuleName { get { return _moduleName; } }
        public long Timestamp { get { return _timestamp; } }

        public AggregateMigrationMetadata(long timestamp, string moduleName)
        {
            _moduleName = moduleName;
            _timestamp = timestamp;
        }
    }
}