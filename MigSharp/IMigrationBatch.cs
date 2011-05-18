using System;

namespace MigSharp
{
    /// <summary>
    /// Represents a batch of <see cref="IReversibleMigration"/>s.
    /// </summary>
    public interface IMigrationBatch
    {
        /// <summary>
        /// Raised after each migration that has been executed.
        /// </summary>
        event EventHandler<MigrationEventArgs> StepExecuting;

        /// <summary>
        /// Raised after each migration that has been executed.
        /// </summary>
        event EventHandler<MigrationEventArgs> StepExecuted;

        /// <summary>
        /// Gets the number of migrations in this batch.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Performs the migrations contained in this batch.
        /// </summary>
        void Execute();
    }
}