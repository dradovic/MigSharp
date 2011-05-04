using System;
using System.Collections.Generic;
using System.Linq;

using MigSharp.Core;

namespace MigSharp.Process
{
    internal class MigrationBatch : IMigrationBatch
    {
        public static readonly IMigrationBatch Empty = new EmptyMigrationBatch();

        private readonly IEnumerable<IMigrationStep> _upMigrations;
        private readonly IEnumerable<IMigrationStep> _downMigrations;
        private readonly IVersioning _versioning;
        private readonly MigrationOptions _options;

        public event EventHandler<MigrationEventArgs> StepExecuted;

        public int Count { get { return _upMigrations.Count() + _downMigrations.Count(); } }

        public MigrationBatch(
            IEnumerable<IMigrationStep> upMigrations,
            IEnumerable<IMigrationStep> downMigrations,
            IVersioning versioning,
            MigrationOptions options)
        {
            _upMigrations = upMigrations;
            _downMigrations = downMigrations;
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
            step.Execute(_versioning);

            OnStepExecuted(new MigrationEventArgs(step.Metadata, step.Direction));
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