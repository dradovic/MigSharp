using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using MigSharp.Process;

using NUnit.Framework;

using Rhino.Mocks;

namespace MigSharp.NUnit.Process
{
    [TestFixture, Category("smoke")]
    public class PersistedVersioningTests
    {
        private const long ExistingTimestampForDefaultModule = 20100609110134;
        private const string ExistingTagForDefaultModule = null;
        private const long ExistingTimestampForTestModule = 20100617183831;
        private const string ExistingTagForTestModule = "I'm a migration of the test module";
        private const string TestModule = "Test Module";

        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "TestIs")]
        [Test]
        public void TestExecutedMigrations()
        {
            PersistedVersioning persistedVersioning = CreateVersioning();
            var executedMigrations = new List<IMigrationMetadata>(persistedVersioning.ExecutedMigrations);
            Assert.AreEqual(2, executedMigrations.Count);
            Assert.AreEqual(ExistingTimestampForDefaultModule, executedMigrations[0].Timestamp);
            Assert.AreEqual(string.Empty, executedMigrations[0].ModuleName);
            Assert.AreEqual(ExistingTagForDefaultModule, executedMigrations[0].Tag);
            Assert.AreEqual(ExistingTimestampForTestModule, executedMigrations[1].Timestamp);
            Assert.AreEqual(TestModule, executedMigrations[1].ModuleName);
            Assert.AreEqual(ExistingTagForTestModule, executedMigrations[1].Tag);
        }

        private static PersistedVersioning CreateVersioning()
        {
            var history = new History("TableName", MockRepository.GenerateStub<IProviderMetadata>());
            history.LoadEntry(ExistingTimestampForDefaultModule, string.Empty, ExistingTagForDefaultModule);
            history.LoadEntry(ExistingTimestampForTestModule, TestModule, ExistingTagForTestModule);
            return new PersistedVersioning(history);
        }
    }
}