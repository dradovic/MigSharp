namespace MigSharp.NUnit.Integration
{
    internal interface IIntegrationTestMigration : IMigration
    {
        ExpectedTables Tables { get; }
    }
}