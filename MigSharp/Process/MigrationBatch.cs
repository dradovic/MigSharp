using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using MigSharp.Core;

namespace MigSharp.Process
{
    internal class MigrationBatch : IMigrationBatch
    {
        private readonly List<IMigrationStep> _migrations;
        private readonly ReadOnlyCollection<IMigrationMetadata> _unidentifiedMigrations;
        private readonly IValidator _validator;
        private readonly IVersioning _versioning;

        public event EventHandler<MigrationEventArgs> StepExecuting;
        public event EventHandler<MigrationEventArgs> StepExecuted;

        private ReadOnlyCollection<IScheduledMigrationMetadata> _scheduledMigrations;
        public ReadOnlyCollection<IScheduledMigrationMetadata> ScheduledMigrations { get { return _scheduledMigrations; } }

        public ReadOnlyCollection<IMigrationMetadata> UnidentifiedMigrations { get { return _unidentifiedMigrations; } }

        public bool IsExecuted { get; private set; }

        public MigrationBatch(
            IEnumerable<IMigrationStep> migrations,
            IEnumerable<IMigrationMetadata> unidentifiedMigrations,
            IValidator validator,
            IVersioning versioning)
        {
            _migrations = migrations.ToList();
            UpdateScheduledMigrations();
            _unidentifiedMigrations = new ReadOnlyCollection<IMigrationMetadata>(unidentifiedMigrations.ToList());
            _validator = validator;
            _versioning = versioning;
        }

        public void RemoveAll(Predicate<IMigrationMetadata> match)
        {
            _migrations.RemoveAll(m => match(m.Metadata));
            UpdateScheduledMigrations();
        }

        private void UpdateScheduledMigrations()
        {
            _scheduledMigrations = new ReadOnlyCollection<IScheduledMigrationMetadata>(_migrations.Select(s => s.Metadata).ToList());
        }

        public void Execute()
        {
            if (IsExecuted) throw new InvalidOperationException("Cannot execute the same batch twice.");
            IsExecuted = true;

            DateTime start = DateTime.Now;

            // validate all steps
            string errors;
            string warnings;
            _validator.Validate(_migrations, out errors, out warnings);
            if (!string.IsNullOrEmpty(errors))
            {
                throw new InvalidOperationException("Cannot execute the migration(s) as there are validation errors:" + Environment.NewLine + errors);
            }
            if (!string.IsNullOrEmpty(warnings))
            {
                Log.Warning(warnings);
            }

            // execute all steps
            foreach (IMigrationStep step in _migrations)
            {
                ExecuteStep(step);
            }

            Debug.Assert(IsExecuted, "At the end of this method _isExecuted must be true.");

            Log.Info(LogCategory.Performance, "Migration and validation of batch took {0}s",
                (DateTime.Now - start).TotalSeconds);
        }

        private void ExecuteStep(IMigrationStep step)
        {
            OnStepExecuting(new MigrationEventArgs(step.Metadata));

            step.Execute(_versioning);

            OnStepExecuted(new MigrationEventArgs(step.Metadata));
        }

        private void OnStepExecuting(MigrationEventArgs e)
        {
            EventHandler<MigrationEventArgs> tmp = StepExecuting;
            if (tmp != null) tmp(this, e);
        }

        private void OnStepExecuted(MigrationEventArgs e)
        {
            EventHandler<MigrationEventArgs> tmp = StepExecuted;
            if (tmp != null) tmp(this, e);
        }
    }
}