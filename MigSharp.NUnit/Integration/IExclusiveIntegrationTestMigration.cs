using System.Collections.Generic;
using System.Linq;

namespace MigSharp.NUnit.Integration
{
    internal interface IExclusiveIntegrationTestMigration : IIntegrationTestMigration
    {
        IEnumerable<string> ProvidersNotSupportingFeatureUnderTest { get; }        
    }

    internal static class ExclusiveIntegrationTestMigrationExtensions
    {
        public static bool IsFeatureSupported(this IExclusiveIntegrationTestMigration migration, IDatabase db)
        {
            return !migration.ProvidersNotSupportingFeatureUnderTest.Contains(db.Context.ProviderMetadata.Name);
        }
    }
}