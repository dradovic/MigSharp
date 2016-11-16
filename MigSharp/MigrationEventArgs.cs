using System;

namespace MigSharp
{
    /// <summary>
    /// Event arguments for migration events.
    /// </summary>
    public class MigrationEventArgs : EventArgs
    {
        private readonly IMigrationStepMetadata _metadata;

        /// <summary>
        /// Gets the associated metadata.
        /// </summary>
        public IMigrationStepMetadata Metadata { get { return _metadata; } }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public MigrationEventArgs(IMigrationStepMetadata metadata)
        {
            _metadata = metadata;
        }
    }
}