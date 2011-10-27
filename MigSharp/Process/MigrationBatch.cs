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
        private readonly IEnumerable<IMigrationStep> _migrations;
        private readonly ReadOnlyCollection<IMigrationMetadata> _unidentifiedMigrations;
        private readonly IVersioning _versioning;
        private readonly MigrationOptions _options;

        public event EventHandler<MigrationEventArgs> StepExecuting;
        public event EventHandler<MigrationEventArgs> StepExecuted;

        private readonly ReadOnlyCollection<IScheduledMigrationMetadata> _scheduledMigrations;
        public ReadOnlyCollection<IScheduledMigrationMetadata> ScheduledMigrations { get { return _scheduledMigrations; } }

        public ReadOnlyCollection<IMigrationMetadata> UnidentifiedMigrations { get { return _unidentifiedMigrations; } }

        public bool IsExecuted { get; private set; }

        public MigrationBatch(
            IEnumerable<IMigrationStep> migrations,
            IEnumerable<IMigrationMetadata> unidentifiedMigrations,
            IVersioning versioning,
            MigrationOptions options)
        {
            _migrations = migrations;
            _scheduledMigrations = new ReadOnlyCollection<IScheduledMigrationMetadata>(_migrations.Select(s => s.Metadata).ToList());
            _unidentifiedMigrations = new ReadOnlyCollection<IMigrationMetadata>(unidentifiedMigrations.ToList());
            _versioning = versioning;
            _options = options;
        }

        public void Execute()
        {
            if (IsExecuted) throw new InvalidOperationException("Cannot execute the same batch twice.");
            IsExecuted = true;

            // validate all steps
            var validator = new Validator(_options);
            string errors;
            string warnings;
// ReSharper disable RedundantEnumerableCastCall
            validator.Validate(_migrations.Cast<IMigrationReporter>(), out errors, out warnings); // .Cast<IMigrationReporter>() is required for .NET 3.5
// ReSharper restore RedundantEnumerableCastCall
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