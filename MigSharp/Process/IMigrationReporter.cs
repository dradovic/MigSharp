namespace MigSharp.Process
{
    internal interface IMigrationReporter
    {
        IScheduledMigrationMetadata MigrationMetadata { get; }
        IMigrationReport Report(IMigrationContext context);
    }
}