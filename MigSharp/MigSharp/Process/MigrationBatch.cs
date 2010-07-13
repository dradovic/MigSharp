using System;
using System.Collections.Generic;
using System.Linq;

namespace MigSharp.Process
{
    internal class MigrationBatch : IMigrationBatch
    {
        public static readonly IMigrationBatch Empty = new EmptyMigrationBatch();

        private readonly IEnumerable<IMigrationStep> _upMigrations;
        private readonly IEnumerable<IMigrationStep> _downMigrations;
        private readonly IVersioning _versioning;

        public event EventHandler<MigrationEventArgs> StepExecuted;

        public int Count { get { return _upMigrations.Count() + _downMigrations.Count(); } }

        public MigrationBatch(
            IEnumerable<IMigrationStep> upMigrations,
            IEnumerable<IMigrationStep> downMigrations,
            IVersioning versioning)
        {
            _upMigrations = upMigrations;
            _downMigrations = downMigrations;
            _versioning = versioning;
        }

        public void Execute()
        {
            foreach (IMigrationStep step in _downMigrations)
            {
                ExecuteStep(step, MigrationDirection.Down);
            }
            foreach (IMigrationStep step in _upMigrations)
            {
                ExecuteStep(step, MigrationDirection.Up);
            }
        }

        private void ExecuteStep(IMigrationStep step, MigrationDirection direction)
        {
            step.Execute(_versioning, direction);

            OnStepExecuted(new MigrationEventArgs(step.Metadata, direction));
        }

        private void OnStepExecuted(MigrationEventArgs e)
        {
            EventHandler<MigrationEventArgs> tmp = StepExecuted;
            if (tmp != null) tmp(this, e);
        }

        internal class EmptyMigrationBatch : IMigrationBatch
        {
            public event EventHandler<MigrationEventArgs> StepExecuted;

            public int Count { get { return 0; } }

            public void Execute()
            {
            }
        }
    }
}