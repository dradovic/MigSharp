using System;
using System.Linq;

using MigSharp.Process;
using MigSharp.Providers;

using NUnit.Framework;

using Rhino.Mocks;

namespace MigSharp.NUnit.Process
{
    [TestFixture, Category("smoke")]
    public class MigrationBatchTests
    {
        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void VerifyValidationErrorsResultInException()
        {
            IMigrationStep step = MockRepository.GenerateStub<IMigrationStep>();
            step.Expect(s => s.Metadata).Return(new Metadata1());
            IMigrationReport erroneousReport = CreateMigrationReport();
            erroneousReport.Expect(r => r.Error).Return("Some test failure...");
            step.Expect(s => s.Report(null)).IgnoreArguments().Return(erroneousReport);
            IMigrationStep[] steps = new[]
            {
                step,
            };
            IVersioning versioning = MockRepository.GenerateStub<IVersioning>();
            MigrationBatch batch = new MigrationBatch(steps, Enumerable.Empty<IMigrationMetadata>(), versioning, new MigrationOptions());

            batch.Execute();
            Assert.IsTrue(batch.IsExecuted);
        }

        [Test]
        public void VerifyStepExecutedAndStepExecutingAreRaised()
        {
            IMigrationStep step = MockRepository.GenerateStub<IMigrationStep>();
            var metadata = new Metadata1();
            step.Expect(s => s.Metadata).Return(metadata);
            step.Expect(s => s.Report(null)).IgnoreArguments().Return(CreateMigrationReport());
            IMigrationStep[] steps = new[]
            {
                step,
            };
            IVersioning versioning = MockRepository.GenerateStub<IVersioning>();
            var batch = new MigrationBatch(steps, Enumerable.Empty<IMigrationMetadata>(), versioning, new MigrationOptions());
            Assert.AreSame(metadata, batch.ScheduledMigrations[0], "The batch should expose the metadata of the step."); // this is tested to allow for the undocumented feature test below
            int countExecutingEvent = 0;
            int countExecutedEvent = 0;
            batch.StepExecuting += (sender, args) =>
                {
                    // note: the following assertion tests an undocumented feature
                    Assert.AreSame(metadata, args.Metadata, "The event should carry the same metadata that is in the ScheduleMigrations collection.");
                    countExecutingEvent++;
                };
            batch.StepExecuted += (sender, args) =>
            {
                // note: the following assertion tests an undocumented feature
                Assert.AreSame(metadata, args.Metadata, "The event should carry the same metadata that is in the ScheduleMigrations collection.");
                countExecutedEvent++;
            };

            batch.Execute();

            Assert.IsTrue(batch.IsExecuted);
            Assert.AreEqual(steps.Length, countExecutingEvent);
            Assert.AreEqual(steps.Length, countExecutedEvent);
        }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void VerifyCallingExecuteTwiceThrows()
        {
            var batch = new MigrationBatch(Enumerable.Empty<IMigrationStep>(), Enumerable.Empty<IMigrationMetadata>(), MockRepository.GenerateStub<IVersioning>(), new MigrationOptions());
            batch.Execute();
            batch.Execute();
        }

        private static IMigrationReport CreateMigrationReport()
        {
            IMigrationReport report = MockRepository.GenerateStub<IMigrationReport>();
            report.Expect(r => r.DataTypes).Return(Enumerable.Empty<DataType>());
            report.Expect(r => r.PrimaryKeyDataTypes).Return(Enumerable.Empty<DataType>());
            report.Expect(r => r.Methods).Return(Enumerable.Empty<string>());
            return report;
        }

        private class Metadata1 : IScheduledMigrationMetadata
        {
            public string Tag { get { return null; } }
            public string ModuleName { get { return string.Empty; } }
            public long Timestamp { get { return 1; } }
            public MigrationDirection Direction { get { return MigrationDirection.Up; } }
        }
    }
}