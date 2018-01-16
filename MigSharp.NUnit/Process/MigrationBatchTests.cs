using System.Collections.Generic;
using System.Linq;
using FakeItEasy;
using MigSharp.Core;
using MigSharp.Process;
using NUnit.Framework;

namespace MigSharp.NUnit.Process
{
    [TestFixture, Category("smoke")]
    public class MigrationBatchTests
    {
        [Test]
        public void VerifyValidationErrorsResultInException()
        {
            string errors;
            string warnings;
            var validator = A.Fake<IValidator>();
            A.CallTo(() => validator.Validate(A<IEnumerable<IMigrationReporter>>._, out errors, out warnings)).AssignsOutAndRefParameters("Some test failure...", null);

            IMigrationStep[] steps = { FakeMigrationStep(new StepMetadataStub(new MigrationMetadata(1, null, null))) };
            var configuration = A.Fake<IRuntimeConfiguration>();
            A.CallTo(() => configuration.Validator).Returns(validator);
            MigrationBatch batch = new MigrationBatch(steps, Enumerable.Empty<IMigrationMetadata>(), A.Fake<IVersioning>(), configuration);

            Assert.That(() => batch.Execute(), Throws.InvalidOperationException);
        }

        [Test]
        public void VerifyStepExecutedAndStepExecutingAreRaised()
        {
            var metadata = new StepMetadataStub(new MigrationMetadata(1, null, null));
            IMigrationStep step = FakeMigrationStep(metadata);
            IMigrationStep[] steps = { step };
            var batch = new MigrationBatch(steps, Enumerable.Empty<IMigrationMetadata>(), A.Fake<IVersioning>(), A.Fake<IRuntimeConfiguration>());

            Assert.AreSame(metadata, batch.Steps[0], "The batch should expose the metadata of the step."); // this is tested to allow for the undocumented feature test below

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

        [Test]
        public void VerifyCallingExecuteTwiceThrows()
        {
            var batch = new MigrationBatch(Enumerable.Empty<IMigrationStep>(), Enumerable.Empty<IMigrationMetadata>(), A.Fake<IVersioning>(), A.Fake<IRuntimeConfiguration>());
            batch.Execute();
            Assert.That(() => batch.Execute(), Throws.InvalidOperationException);
        }

        private static IMigrationStep FakeMigrationStep(IMigrationStepMetadata metadata)
        {
            IMigrationStep step = A.Fake<IMigrationStep>();
            A.CallTo(() => step.Metadata).Returns(metadata);
            A.CallTo(() => step.Report(A<IMigrationContext>._)).Returns(A.Fake<IMigrationReport>());
            return step;
        }

        private class StepMetadataStub : IMigrationStepMetadata
        {
            private readonly List<IMigrationMetadata> _metadatas;

            public string ModuleName { get { return string.Empty; } }
            public MigrationDirection Direction { get { return MigrationDirection.Up; } }
            public bool UseModuleNameAsDefaultSchema { get { return false; } }
            public IEnumerable<IMigrationMetadata> Migrations { get { return _metadatas; } }

            public StepMetadataStub(params IMigrationMetadata[] metadatas)
            {
                _metadatas = new List<IMigrationMetadata>(metadatas);
            }
        }
    }
}