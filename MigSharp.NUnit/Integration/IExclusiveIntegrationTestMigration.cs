using System.Collections.Generic;
using System.Linq;

namespace MigSharp.NUnit.Integration
{
    internal interface IExclusiveIntegrationTestMigration : IIntegrationTestMigration
    {
        IEnumerable<Platform> PlatformsNotSupportingFeatureUnderTest { get; }

    }

    internal interface IVersionConstrainedExclusiveIntegrationTestMigration : IExclusiveIntegrationTestMigration
    {
        DbPlatform MinimumVersionSupportingFeatureUnderTest(Platform platform);
    }

    internal static class ExclusiveIntegrationTestMigrationExtensions
    {
        public static bool IsFeatureSupported(this IExclusiveIntegrationTestMigration migration, IDatabase db)
        {
            if (migration.PlatformsNotSupportingFeatureUnderTest.Contains(db.Context.ProviderMetadata.Platform))
            {
                return false;
            }
            IVersionConstrainedExclusiveIntegrationTestMigration constrainedMigration = migration as IVersionConstrainedExclusiveIntegrationTestMigration;
            if (constrainedMigration != null)
            {
                return db.Context.ProviderMetadata.MajorVersion >= constrainedMigration.MinimumVersionSupportingFeatureUnderTest(db.Context.ProviderMetadata.Platform).MajorVersion;
            }
            return true;
        }
    }
}