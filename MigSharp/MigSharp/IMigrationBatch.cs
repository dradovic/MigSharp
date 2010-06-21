using System;

using MigSharp.Process;

namespace MigSharp
{
    /// <summary>
    /// Represents a batch of <see cref="IMigration"/>s.
    /// </summary>
    public interface IMigrationBatch
    {
        event EventHandler<MigrationEventArgs> MigrationFinished;
        //event EventHandler<CancelableMigrationEventArgs> MigrationStarting;

        /// <summary>
        /// Gets the number of migrations in this batch.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Performs the migrations contained in this batch.
        /// </summary>
        void Execute();
    }

    //public class CancelableMigrationEventArgs : MigrationEventArgs
    //{
    //    public bool Cancel { get; set; }

    //    public CancelableMigrationEventArgs(IMigrationMetadata metadata, MigrationDirection direction) : base(metadata, direction)
    //    {
    //    }
    //}

    public class MigrationEventArgs : EventArgs
    {
        private readonly IMigrationMetadata _metadata;
        private readonly MigrationDirection _direction;

        public IMigrationMetadata Metadata { get { return _metadata; } }
        public MigrationDirection Direction { get { return _direction; } }

        public MigrationEventArgs(IMigrationMetadata metadata, MigrationDirection direction)
        {
            _metadata = metadata;
            _direction = direction;
        }
    }
}