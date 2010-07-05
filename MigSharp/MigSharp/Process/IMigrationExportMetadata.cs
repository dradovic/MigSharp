namespace MigSharp.Process
{
    public interface IMigrationExportMetadata
    {
        string Tag { get; }
        string ModuleName { get; }
    }
}