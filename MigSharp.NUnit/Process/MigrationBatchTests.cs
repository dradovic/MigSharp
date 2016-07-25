using System;
using System.Collections.Generic;
using System.Linq;
using FakeItEasy;
using MigSharp.Process;
using NUnit.Framework;

namespace MigSharp.NUnit.Process
{
    [TestFixture, Category("smoke")]
    public class MigrationBatchTests
    {
        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void VerifyValidationErrorsResultInException()
        {
            string errors;
            string warnings;
            var validator = A.Fake<IValidator>();
            A.CallTo(() => validator.Validate(A<IEnumerable<IMigrationReporter>>._, out errors, out warnings)).AssignsOutAndRefParameters("Some test failure...", null);

            IMigrationStep[] steps = { FakeMigrationStep(new MetadataStub(1)) };
            MigrationBatch batch = new MigrationBatch(steps, Enumerable.Empty<IMigrationMetadata>(), validator, A.Fake<IVersioning>());

            batch.Execute();
            Assert.IsTrue(batch.IsExecuted);
        }

        [Test]
        public void VerifyStepExecutedAndStepExecutingAreRaised()
        {
            var metadata = new MetadataStub(1);
            IMigrationStep step = FakeMigrationStep(metadata);
            IMigrationStep[] steps = { step };
            var batch = new MigrationBatch(steps, Enumerable.Empty<IMigrationMetadata>(), A.Fake<IValidator>(), A.Fake<IVersioning>());

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
            var batch = new MigrationBatch(Enumerable.Empty<IMigrationStep>(), Enumerable.Empty<IMigrationMetadata>(), A.Fake<IValidator>(), A.Fake<IVersioning>());
            batch.Execute();
            batch.Execute();
        }

        [Test]
        public void TestRemovingMigrations()
        {
            IMigrationStep[] steps =
            {
                FakeMigrationStep(new MetadataStub(1)),
                FakeMigrationStep(new MetadataStub(2)),
                FakeMigrationStep(new MetadataStub(3))
            };
            var batch = new MigrationBatch(steps, Enumerable.Empty<IMigrationMetadata>(), A.Fake<IValidator>(), A.Fake<IVersioning>());
            Assert.AreEqual(3, batch.ScheduledMigrations.Count);

            batch.RemoveAll(m => m.Timestamp == 2);
            Assert.AreEqual(2, batch.ScheduledMigrations.Count);

            int countExecutedEvent = 0;
            batch.StepExecuted += (sender, args) =>
            {
                Assert.AreNotEqual(2, args.Metadata.Timestamp, "Migration with timestamp 2 should not have been executed.");
                countExecutedEvent++;
            };
            batch.Execute();

            Assert.IsTrue(batch.IsExecuted);
            Assert.AreEqual(2, countExecutedEvent);
        }

        private static IMigrationStep FakeMigrationStep(IScheduledMigrationMetadata metadata)
        {
            IMigrationStep step = A.Fake<IMigrationStep>();
            A.CallTo(() => step.Metadata).Returns(metadata);
            A.CallTo(() => step.Report(A<IMigrationContext>._)).Returns(A.Fake<IMigrationReport>());
            return step;
        }

        private class MetadataStub : IScheduledMigrationMetadata
        {
            public string Tag { get { return null; } }
            public string ModuleName { get { return string.Empty; } }
            public long Timestamp { get; private set; }
            public MigrationDirection Direction { get { return MigrationDirection.Up; } }
            public bool UseModuleNameAsDefaultSchema { get { return false; } }

            public MetadataStub(long timestamp)
            {
                Timestamp = timestamp;
            }
        }
    }
}