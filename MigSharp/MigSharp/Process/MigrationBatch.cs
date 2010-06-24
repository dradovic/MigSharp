using System;
using System.Collections.Generic;
using System.Globalization;

using MigSharp.Core;

using System.Linq;

namespace MigSharp.Process
{
    internal class MigrationBatch : IMigrationBatch
    {
        public static readonly IMigrationBatch Empty = new EmptyMigrationBatch();

        private readonly IEnumerable<IMigrationStep> _upMigrations;
        private readonly IEnumerable<IMigrationStep> _downMigrations;
        private readonly IDbVersion _dbVersion;

        // TODO: unit test events
        public event EventHandler<MigrationEventArgs> StepExecuted;
        //public event EventHandler<CancelableMigrationEventArgs> MigrationStarting;

        public int Count { get { return _upMigrations.Count() + _downMigrations.Count(); } }

        public MigrationBatch(
            IEnumerable<IMigrationStep> upMigrations,
            IEnumerable<IMigrationStep> downMigrations, 
            IDbVersion dbVersion)
        {
            _upMigrations = upMigrations;
            _downMigrations = downMigrations;
            _dbVersion = dbVersion;
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
            DateTime start = DateTime.Now;

            step.Execute(_dbVersion, direction);

            Log.Info(LogCategory.Performance, "Migration to {0}{1}{2} took {3}s",
                step.Metadata.Timestamp(),
                !string.IsNullOrEmpty(step.Metadata.ModuleName) ? string.Format(CultureInfo.CurrentCulture, " [{0}]", step.Metadata.ModuleName) : string.Empty,
                !string.IsNullOrEmpty(step.Metadata.Tag) ? string.Format(CultureInfo.CurrentCulture, " '{0}'", step.Metadata.Tag) : string.Empty,
                (DateTime.Now - start).TotalSeconds);

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
            //public event EventHandler<CancelableMigrationEventArgs> MigrationStarting;

            public int Count { get { return 0; } }

            public void Execute()
            {
            }
        }
    }
}