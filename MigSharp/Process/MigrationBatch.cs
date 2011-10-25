using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using MigSharp.Core;

namespace MigSharp.Process
{
    internal class MigrationBatch : IMigrationBatch
    {
        private readonly IEnumerable<IMigrationStep> _upMigrations;
        private readonly IEnumerable<IMigrationStep> _downMigrations;
        private readonly ReadOnlyCollection<IMigrationMetadata> _unidentifiedMigrations;
        private readonly IVersioning _versioning;
        private readonly MigrationOptions _options;

        public event EventHandler<MigrationEventArgs> StepExecuting;
        public event EventHandler<MigrationEventArgs> StepExecuted;

        private readonly ReadOnlyCollection<IScheduledMigrationMetadata> _scheduledMigrations;
        public ReadOnlyCollection<IScheduledMigrationMetadata> ScheduledMigrations { get { return _scheduledMigrations; } }

        public ReadOnlyCollection<IMigrationMetadata> UnidentifiedMigrations { get { return _unidentifiedMigrations; } }

        public MigrationBatch(
            IEnumerable<IMigrationStep> upMigrations,
            IEnumerable<IMigrationStep> downMigrations,
            IEnumerable<IMigrationMetadata> unidentifiedMigrations,
            IVersioning versioning,
            MigrationOptions options)
        {
            _upMigrations = upMigrations;
            _downMigrations = downMigrations;
            _scheduledMigrations = new ReadOnlyCollection<IScheduledMigrationMetadata>(
                _downMigrations.Select(s => s.Metadata).Concat(_upMigrations.Select(s => s.Metadata)).ToList());
            _unidentifiedMigrations = new ReadOnlyCollection<IMigrationMetadata>(unidentifiedMigrations.ToList());
            _versioning = versioning;
            _options = options;
        }

        public void Execute()
        {
            // validate all steps
            var validator = new Validator(_options);
            string errors;
            string warnings;
// ReSharper disable RedundantEnumerableCastCall
            validator.Validate(_downMigrations.Concat(_upMigrations).Cast<IMigrationReporter>(), out errors, out warnings); // .Cast<IMigrationReporter>() is required for .NET 3.5
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
            foreach (IMigrationStep step in _downMigrations)
            {
                ExecuteStep(step);
            }
            foreach (IMigrationStep step in _upMigrations)
            {
                ExecuteStep(step);
            }
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