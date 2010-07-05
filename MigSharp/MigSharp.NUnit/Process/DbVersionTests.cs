using System.Collections.Generic;

using MigSharp.Process;

using NUnit.Framework;

using Rhino.Mocks;

namespace MigSharp.NUnit.Process
{
    [TestFixture, Category("Smoke")]
    public class DbVersionTests
    {
        private const long ExistingTimestampForDefaultModule = 20100609110134;
        private const long ExistingTimestampForTestModule = 20100617183831;
        private const string TestModule = "Test Module";

        [Test, TestCaseSource("GetCasesForIsContained")]
        public bool TestIsContained(object metaData)
        {
            DbVersion dbVersion = CreateDbVersion();
            return dbVersion.IsContained((IMigrationMetadata)metaData);
        }

// ReSharper disable UnusedMember.Local
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

        private static DbVersion CreateDbVersion()
        {
            var ds = new DbVersionDataSet();
            ds.DbVersion.AddDbVersionRow(ExistingTimestampForDefaultModule, string.Empty, null);
            ds.DbVersion.AddDbVersionRow(ExistingTimestampForTestModule, TestModule, null);
            return DbVersion.Create(ds);
        }
    }
}