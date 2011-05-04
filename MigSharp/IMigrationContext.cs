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
        IProviderMetadata ProviderMetadata { get; }
    }
}