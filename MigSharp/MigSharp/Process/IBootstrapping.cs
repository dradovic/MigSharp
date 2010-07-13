namespace MigSharp.Process
{
    public interface IBootstrapping
    {
        /// <summary>
        /// Returns true if the migration <paramref name="migrationMetadata"/> should be assumed as already executed when bootstrapping the versioning.
        /// </summary>
        bool IsContained(IMigrationMetadata migrationMetadata);
    }
}