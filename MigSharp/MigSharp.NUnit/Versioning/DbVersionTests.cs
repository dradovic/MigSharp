using System;
using System.Collections.Generic;

using MigSharp.Versioning;

using NUnit.Framework;

using Rhino.Mocks;

namespace MigSharp.NUnit.Versioning
{
    [TestFixture]
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
            existingMigration.Expect(m => m.Timestamp).Return(timeStamp);
            return existingMigration;
        }

        private static DbVersion CreateDbVersion()
        {
            var ds = new DbVersionDataSet();
            ds.DbVersion.AddDbVersionRow(ExistingTimestamp, null, null);
            return DbVersion.Create(ds);
        }
    }
}