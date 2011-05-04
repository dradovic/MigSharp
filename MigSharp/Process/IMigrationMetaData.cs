namespace MigSharp.Process
{
    public interface IMigrationMetadata : IMigrationExportMetadata
    {
        long Timestamp { get; }
    }
}