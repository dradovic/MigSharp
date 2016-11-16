namespace MigSharp.Process
{
    internal interface IMigrationReporter
    {
        IMigrationStepMetadata StepMetadata { get; }
        IMigrationReport Report(IMigrationContext context);
    }
}