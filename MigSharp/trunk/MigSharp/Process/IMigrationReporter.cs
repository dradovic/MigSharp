namespace MigSharp.Process
{
    internal interface IMigrationReporter
    {
        IMigrationReport Report(IMigrationContext context);
    }
}