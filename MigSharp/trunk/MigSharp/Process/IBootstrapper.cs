using System.Data;

namespace MigSharp.Process
{
    /// <summary>
    /// Implements a custom bootstrapping logic which is executed if the versioning table of MigSharp doe not exists yet.
    /// </summary>
    public interface IBootstrapper
    {
        /// <summary>
        /// Triggers whatever actions are needed to prepare the custom bootstrapping. This method is called exactly once,
        /// before <see cref="IsContained"/> is called any times.
        /// </summary>
        /// <param name="connection">The connection used to update the versioning table.</param>
        /// <param name="transaction">The transaction used to update the versioning table.</param>
        void BeginBootstrapping(IDbConnection connection, IDbTransaction transaction);

        /// <summary>
        /// Returns true if the migration <paramref name="metadata"/> should be assumed as already executed when bootstrapping the versioning.
        /// </summary>
        bool IsContained(IMigrationMetadata metadata);

        /// <summary>
        /// Triggers whatever actions are needed to finish the custom bootstrapping. This method is called exactly once,
        /// after <see cref="IsContained"/> is called any times.
        /// </summary>
        /// <param name="connection">The connection used to update the versioning table.</param>
        /// <param name="transaction">The transaction used to update the versioning table.</param>
        void EndBootstrapping(IDbConnection connection, IDbTransaction transaction);
    }
}