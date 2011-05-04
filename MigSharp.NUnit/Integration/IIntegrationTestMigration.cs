namespace MigSharp.NUnit.Integration
{
    internal interface IIntegrationTestMigration : IMigration
    {
        /// <summary>
        /// Gets the name of the created table.
        /// </summary>
        string TableName { get; }

        string[] ColumnNames { get; }

        object[,] ExpectedValues { get; }
    }
}