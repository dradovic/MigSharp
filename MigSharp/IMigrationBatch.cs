using System;
using System.Collections.ObjectModel;

namespace MigSharp
{
    /// <summary>
    /// Represents a batch of <see cref="IReversibleMigration"/>s.
    /// </summary>
    public interface IMigrationBatch
    {
        /// <summary>
        /// Raised before each migration that will be executed.
        /// </summary>
        event EventHandler<MigrationEventArgs> StepExecuting;

        /// <summary>
        /// Raised after each migration that has been executed.
        /// </summary>
        event EventHandler<MigrationEventArgs> StepExecuted;

        /// <summary>
        /// Gets a list of migrations which are pending and will be executed
        /// when calling <see cref="Execute"/>.
        /// </summary>
        ReadOnlyCollection<IScheduledMigrationMetadata> ScheduledMigrations { get; }

        /// <summary>
        /// Gets a list of migrations which were executed server-side but
        /// are not found in the application.
        /// <para>
        /// Use this property to find out if the application is out-of-date
        /// compared to the actual schema of the database.
        /// </para>
        /// </summary>
        ReadOnlyCollection<IMigrationMetadata> UnidentifiedMigrations { get; }

        /// <summary>
        /// Removes migrations selected by <paramref name="match"/> from this batch.
        /// <param name="match">Function to select migrations to remove.</param>
        /// </summary>
        void RemoveAll(Predicate<IMigrationMetadata> match);

        /// <summary>
        /// Performs the migrations contained in this batch. This method can only be
        /// called once (when IsExecuted is false).
        /// </summary>
        void Execute();

        /// <summary>
        /// Indicates if <see cref="Execute"/> was already called.
        /// </summary>
        bool IsExecuted { get; }
    }
}