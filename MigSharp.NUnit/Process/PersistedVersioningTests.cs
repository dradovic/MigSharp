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
        private const long ExistingTimestampForTestModule = 20100617183831;
        private const string TestModule = "Test Module";

        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "TestIs")]
        [Test, TestCaseSource("GetCasesForIsContained")]
        public bool TestIsContained(object metadata)
        {
            PersistedVersioning persistedVersioning = CreateVersioning();
            return persistedVersioning.IsContained((IMigrationMetadata)metadata);
        }

// ReSharper disable UnusedMember.Local
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private static IEnumerable<TestCaseData> GetCasesForIsContained()
// ReSharper restore UnusedMember.Local
        {
            IMigrationMetadata migration = GetMigrationMetadata(ExistingTimestampForDefaultModule, string.Empty);
            yield return new TestCaseData(migration)
                .SetDescription("IsContained should be true for existing timestamps")
                .Returns(true);

            migration = GetMigrationMetadata(ExistingTimestampForDefaultModule + 1, string.Empty);
            yield return new TestCaseData(migration)
                .SetDescription("IsContained should be false for future missing timestamps")
                .Returns(false);

            migration = GetMigrationMetadata(ExistingTimestampForDefaultModule - 1, string.Empty);
            yield return new TestCaseData(migration)
                .SetDescription("IsContained should be false for past missing timestamps")
                .Returns(false);

            migration = GetMigrationMetadata(ExistingTimestampForTestModule, TestModule);
            yield return new TestCaseData(migration)
                .SetDescription("IsContained should be true for existing timestamps (Test Module)")
                .Returns(true);

            migration = GetMigrationMetadata(ExistingTimestampForTestModule + 1, TestModule);
            yield return new TestCaseData(migration)
                .SetDescription("IsContained should be false for future missing timestamps (Test Module)")
                .Returns(false);

            migration = GetMigrationMetadata(ExistingTimestampForTestModule - 1, TestModule);
            yield return new TestCaseData(migration)
                .SetDescription("IsContained should be false for past missing timestamps (Test Module)")
                .Returns(false);

            migration = GetMigrationMetadata(ExistingTimestampForTestModule, string.Empty);
            yield return new TestCaseData(migration)
                .SetDescription("IsContained should be false for existing timestamps of another module")
                .Returns(false);

            migration = GetMigrationMetadata(ExistingTimestampForDefaultModule, TestModule);
            yield return new TestCaseData(migration)
                .SetDescription("IsContained should be false for existing timestamps of another module")
                .Returns(false);
        }

        private static IMigrationMetadata GetMigrationMetadata(long timestamp, string module)
        {
            IMigrationMetadata existingMigration = MockRepository.GenerateStub<IMigrationMetadata>();
            existingMigration.Expect(m => m.Timestamp).Return(timestamp);
            existingMigration.Expect(m => m.ModuleName).Return(module);
            return existingMigration;
        }

        private static PersistedVersioning CreateVersioning()
        {
            var history = new History("TableName", MockRepository.GenerateStub<IProviderMetadata>());
            history.LoadEntry(ExistingTimestampForDefaultModule, string.Empty);
            history.LoadEntry(ExistingTimestampForTestModule, TestModule);
            return new PersistedVersioning(history);
        }
    }
}