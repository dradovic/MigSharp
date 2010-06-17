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
        private static readonly DateTime ExistingTimestamp = new DateTime(2010, 06, 09, 11, 01, 34);

        [Test, TestCaseSource("GetCasesForIncludes")]
        public bool TestIncludes(object metaData)
        {
            DbVersion dbVersion = CreateDbVersion();
            return dbVersion.Includes((IMigrationMetaData)metaData);
        }

// ReSharper disable UnusedMember.Local
        private IEnumerable<TestCaseData> GetCasesForIncludes()
// ReSharper restore UnusedMember.Local
        {
            IMigrationMetaData migration = GetMigrationMetaData(ExistingTimestamp);
            yield return new TestCaseData(migration)
                .SetDescription("Includes should be true for existing timestamps")
                .Returns(true);

            migration = GetMigrationMetaData(ExistingTimestamp.AddDays(1));
            yield return new TestCaseData(migration)
                .SetDescription("Includes should be false for future missing timestamps")
                .Returns(false);

            migration = GetMigrationMetaData(ExistingTimestamp.AddDays(-1));
            yield return new TestCaseData(migration)
                .SetDescription("Includes should be false for past missing timestamps")
                .Returns(false);
        }

        private static IMigrationMetaData GetMigrationMetaData(DateTime timeStamp)
        {
            IMigrationMetaData existingMigration = MockRepository.GenerateStub<IMigrationMetaData>();
            existingMigration.Expect(m => m.Year).Return(timeStamp.Year);
            existingMigration.Expect(m => m.Month).Return(timeStamp.Month);
            existingMigration.Expect(m => m.Day).Return(timeStamp.Day);
            existingMigration.Expect(m => m.Hour).Return(timeStamp.Hour);
            existingMigration.Expect(m => m.Minute).Return(timeStamp.Minute);
            existingMigration.Expect(m => m.Second).Return(timeStamp.Second);
            return existingMigration;
        }

        private static DbVersion CreateDbVersion()
        {
            var ds = new DbVersionDataSet();
            ds.DbVersion.AddDbVersionRow(ExistingTimestamp, null, string.Empty);
            return DbVersion.Create(ds);
        }
    }
}