using System;
using System.Collections.Generic;

using MigSharp.Process;

using NUnit.Framework;

using Rhino.Mocks;

namespace MigSharp.NUnit.Process
{
    [TestFixture, Category("Smoke")]
    public class DbVersionTests
    {
        private static readonly DateTime ExistingTimestampForDefaultModule = new DateTime(2010, 06, 09, 11, 01, 34);
        private static readonly DateTime ExistingTimestampForTestModule = new DateTime(2010, 06, 17, 18, 38, 31);
        private const string TestModule = "Test Module";

        [Test, TestCaseSource("GetCasesForIncludes")]
        public bool TestIncludes(object metaData)
        {
            DbVersion dbVersion = CreateDbVersion();
            return dbVersion.Includes((IMigrationMetadata)metaData);
        }

// ReSharper disable UnusedMember.Local
        private static IEnumerable<TestCaseData> GetCasesForIncludes()
// ReSharper restore UnusedMember.Local
        {
            IMigrationMetadata migration = GetMigrationMetadata(ExistingTimestampForDefaultModule, string.Empty);
            yield return new TestCaseData(migration)
                .SetDescription("Includes should be true for existing timestamps")
                .Returns(true);

            migration = GetMigrationMetadata(ExistingTimestampForDefaultModule.AddDays(1), string.Empty);
            yield return new TestCaseData(migration)
                .SetDescription("Includes should be false for future missing timestamps")
                .Returns(false);

            migration = GetMigrationMetadata(ExistingTimestampForDefaultModule.AddDays(-1), string.Empty);
            yield return new TestCaseData(migration)
                .SetDescription("Includes should be false for past missing timestamps")
                .Returns(false);

            migration = GetMigrationMetadata(ExistingTimestampForTestModule, TestModule);
            yield return new TestCaseData(migration)
                .SetDescription("Includes should be true for existing timestamps (Test Module)")
                .Returns(true);

            migration = GetMigrationMetadata(ExistingTimestampForTestModule.AddDays(1), TestModule);
            yield return new TestCaseData(migration)
                .SetDescription("Includes should be false for future missing timestamps (Test Module)")
                .Returns(false);

            migration = GetMigrationMetadata(ExistingTimestampForTestModule.AddDays(-1), TestModule);
            yield return new TestCaseData(migration)
                .SetDescription("Includes should be false for past missing timestamps (Test Module)")
                .Returns(false);

            migration = GetMigrationMetadata(ExistingTimestampForTestModule, string.Empty);
            yield return new TestCaseData(migration)
                .SetDescription("Includes should be false for existing timestamps of another module")
                .Returns(false);

            migration = GetMigrationMetadata(ExistingTimestampForDefaultModule, TestModule);
            yield return new TestCaseData(migration)
                .SetDescription("Includes should be false for existing timestamps of another module")
                .Returns(false);
        }

        private static IMigrationMetadata GetMigrationMetadata(DateTime timeStamp, string module)
        {
            IMigrationMetadata existingMigration = MockRepository.GenerateStub<IMigrationMetadata>();
            existingMigration.Expect(m => m.Year).Return(timeStamp.Year);
            existingMigration.Expect(m => m.Month).Return(timeStamp.Month);
            existingMigration.Expect(m => m.Day).Return(timeStamp.Day);
            existingMigration.Expect(m => m.Hour).Return(timeStamp.Hour);
            existingMigration.Expect(m => m.Minute).Return(timeStamp.Minute);
            existingMigration.Expect(m => m.Second).Return(timeStamp.Second);
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