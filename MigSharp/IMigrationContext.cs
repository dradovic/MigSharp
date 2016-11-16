using JetBrains.Annotations;

namespace MigSharp
{
    /// <summary>
    /// Represents the context of a migration.
    /// </summary>
    public interface IMigrationContext
    {
        /// <summary>
        /// Gets the metadata describing the underlying provider.
        /// </summary>
        [NotNull]
        IProviderMetadata ProviderMetadata { get; }

        /// <summary>
        /// Gets the migration step metadata. Only available in the context of versioned migrations.
        /// </summary>
        IMigrationStepMetadata StepMetadata { get; }
    }
}