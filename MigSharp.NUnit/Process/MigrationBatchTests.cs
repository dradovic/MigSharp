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
            MigrationBatch batch = new MigrationBatch(steps, steps, versioning, new MigrationOptions());

            batch.Execute();
        }

        [Test]
        public void VerifyStepExecutingIsRaised()
        {
            IMigrationStep step = MockRepository.GenerateStub<IMigrationStep>();
            step.Expect(s => s.Metadata).Return(new Metadata1());
            step.Expect(s => s.Report(null)).IgnoreArguments().Return(CreateMigrationReport());
            IMigrationStep[] steps = new[]
            {
                step,
            };
            IVersioning versioning = MockRepository.GenerateStub<IVersioning>();
            MigrationBatch batch = new MigrationBatch(steps, steps, versioning, new MigrationOptions());
            int count = 0;
            batch.StepExecuting += (sender, args) => count++;

            batch.Execute();

            Assert.AreEqual(2 * steps.Length, count);
        }

        [Test]
        public void VerifyStepExecutedIsRaised()
        {
            IMigrationStep step = MockRepository.GenerateStub<IMigrationStep>();
            step.Expect(s => s.Metadata).Return(new Metadata1());
            step.Expect(s => s.Report(null)).IgnoreArguments().Return(CreateMigrationReport());
            IMigrationStep[] steps = new[]
            {
                step,
            };
            IVersioning versioning = MockRepository.GenerateStub<IVersioning>();
            MigrationBatch batch = new MigrationBatch(steps, steps, versioning, new MigrationOptions());
            int count = 0;
            batch.StepExecuted += (sender, args) => count++;

            batch.Execute();

            Assert.AreEqual(2 * steps.Length, count);
        }

        private static IMigrationReport CreateMigrationReport()
        {
            IMigrationReport report = MockRepository.GenerateStub<IMigrationReport>();
            report.Expect(r => r.DataTypes).Return(Enumerable.Empty<DataType>());
            report.Expect(r => r.PrimaryKeyDataTypes).Return(Enumerable.Empty<DataType>());
            report.Expect(r => r.Methods).Return(Enumerable.Empty<string>());
            return report;
        }

        private class Metadata1 : IMigrationMetadata
        {
            public string Tag { get { return null; } }
            public string ModuleName { get { return string.Empty; } }
            public long Timestamp { get { return 1; } }
        }
    }
}