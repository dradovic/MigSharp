using MigSharp.Process;

using NUnit.Framework;

using Rhino.Mocks;

namespace MigSharp.NUnit.Process
{
    [TestFixture, Category("Smoke")]
    public class MigrationBatchTests
    {
        [Test]
        public void VerifyStepExecutedIsRaised()
        {
            IMigrationStep step1 = MockRepository.GenerateStub<IMigrationStep>();
            step1.Expect(s => s.Metadata).Return(new Metadata1());
            IMigrationStep[] steps = new[]
            {
                step1,
            };
            IDbVersion dbVersion = MockRepository.GenerateStub<IDbVersion>();
            MigrationBatch batch = new MigrationBatch(steps, steps, dbVersion);
            int count = 0;
            batch.StepExecuted += (sender, args) => count++;
            batch.Execute();
            Assert.AreEqual(2 * steps.Length, count);
        }

        private class Metadata1 : IMigrationMetadata
        {
            public int Year { get { return 2010; } }
            public int Month { get { return 6; } }
            public int Day { get { return 24; } }
            public int Hour { get { return 10; } }
            public int Minute { get { return 22; } }
            public int Second { get { return 21; } }
            public string Tag { get { return null; } }
            public string ModuleName { get { return string.Empty; } }
        }
    }
}