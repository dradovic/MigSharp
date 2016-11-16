namespace MigSharp.Core
{
    internal class MigrationMetadata : IMigrationMetadata
    {
        private readonly long _timestamp;
        private readonly string _moduleName;
        private readonly string _tag;

        public long Timestamp { get { return _timestamp; } }
        public string ModuleName { get { return _moduleName; } }
        public string Tag { get { return _tag; } }

        public MigrationMetadata(long timestamp, string moduleName, string tag)
        {
            _timestamp = timestamp;
            _moduleName = moduleName;
            _tag = tag;
        }
    }
}