namespace MigSharp.Core
{
    internal interface IAggregateMigrationMetadata
    {
        string ModuleName { get; }
        long Timestamp { get; }
    }
}