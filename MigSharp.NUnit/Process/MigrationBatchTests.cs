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
            IMigrationStep step = A.Fake<IMigrationStep>();
            A.CallTo(() => step.Metadata).Returns(new Metadata1());
            var validator = A.Fake<IValidator>();
            string errors;
            string warnings;
            A.CallTo(() => validator.Validate(A<IEnumerable<IMigrationReporter>>._, out errors, out warnings)).AssignsOutAndRefParameters("Some test failure...", null);
            IMigrationStep[] steps = new[]
            {
                step,
            };
            IVersioning versioning = A.Fake<IVersioning>();
            MigrationBatch batch = new MigrationBatch(steps, Enumerable.Empty<IMigrationMetadata>(), validator, versioning);

            batch.Execute();
            Assert.IsTrue(batch.IsExecuted);
        }

        [Test]
        public void VerifyStepExecutedAndStepExecutingAreRaised()
        {
            IMigrationStep step = A.Fake<IMigrationStep>();
            var metadata = new Metadata1();
            A.CallTo(() => step.Metadata).Returns(metadata);
            A.CallTo(() => step.Report(A<IMigrationContext>._)).Returns(A.Fake<IMigrationReport>());
            IMigrationStep[] steps = new[]
            {
                step,
            };
            IVersioning versioning = A.Fake<IVersioning>();
            var batch = new MigrationBatch(steps, Enumerable.Empty<IMigrationMetadata>(), A.Fake<IValidator>(), versioning);
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

        private class Metadata1 : IScheduledMigrationMetadata
        {
            public string Tag { get { return null; } }
            public string ModuleName { get { return string.Empty; } }
            public long Timestamp { get { return 1; } }
            public MigrationDirection Direction { get { return MigrationDirection.Up; } }
            public bool UseModuleNameAsDefaultSchema { get { return false; } }
        }
    }
}