using System;

using MigSharp.Process;

namespace MigSharp
{
    /// <summary>
    /// Event arguments for migration events.
    /// </summary>
    public class MigrationEventArgs : EventArgs
    {
        private readonly IMigrationMetadata _metadata;
        private readonly MigrationDirection _direction;

        /// <summary>
        /// Gets the associated metadata.
        /// </summary>
        public IMigrationMetadata Metadata { get { return _metadata; } }

        /// <summary>
        /// Gets the direction of the migration step.
        /// </summary>
        public MigrationDirection Direction { get { return _direction; } }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public MigrationEventArgs(IMigrationMetadata metadata, MigrationDirection direction)
        {
            _metadata = metadata;
            _direction = direction;
        }
    }
}