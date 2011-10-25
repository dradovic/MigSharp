using System;

namespace MigSharp
{
    /// <summary>
    /// Event arguments for migration events.
    /// </summary>
    public class MigrationEventArgs : EventArgs
    {
        private readonly IScheduledMigrationMetadata _metadata;

        /// <summary>
        /// Gets the associated metadata.
        /// </summary>
        public IScheduledMigrationMetadata Metadata { get { return _metadata; } } // FIXME: dr, test of ref. eq. of this with the instance contained in Batch.ScheduledMigs?

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public MigrationEventArgs(IScheduledMigrationMetadata metadata)
        {
            _metadata = metadata;
        }
    }
}