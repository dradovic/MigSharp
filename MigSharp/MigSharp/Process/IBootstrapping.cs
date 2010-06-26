namespace MigSharp.Process
{
    public interface IBootstrapping
    {
        bool IsContained(IMigrationMetadata migrationMetadata);
    }
}