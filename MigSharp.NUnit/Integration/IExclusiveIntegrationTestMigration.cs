using System.Collections.Generic;
using System.Linq;

namespace MigSharp.NUnit.Integration
{
    internal interface IExclusiveIntegrationTestMigration : IIntegrationTestMigration
    {
        IEnumerable<Platform> PlatformsNotSupportingFeatureUnderTest { get; }
    }

    internal static class ExclusiveIntegrationTestMigrationExtensions
    {
        public static bool IsFeatureSupported(this IExclusiveIntegrationTestMigration migration, IDatabase db)
        {
            return !migration.PlatformsNotSupportingFeatureUnderTest.Contains(db.Context.ProviderMetadata.Platform);
        }
    }
}