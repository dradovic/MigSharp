namespace MigSharp.Process
{
    internal class BootstrapMetadata : IMigrationMetadata
    {
        public string Tag { get { return "Bootstrapping"; } }
        public string ModuleName { get { return "MigSharp"; } }
        public long Timestamp { get { return 0; } }
    }
}