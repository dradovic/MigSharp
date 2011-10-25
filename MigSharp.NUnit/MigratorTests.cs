using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

using MigSharp.NUnit.Integration;
using MigSharp.Process;

using NUnit.Framework;

using Rhino.Mocks;

namespace MigSharp.NUnit
{
    [TestFixture, Category("smoke")]
    public class MigratorTests
    {
        [Test, TestCaseSource("GetCasesForPendingMigrationsAreFound")]
        public int VerifyPendingMigrationsAreFound(bool migration1IsContained, bool migration2IsContained, bool migration3IsContained)
        {
            IVersioning versioning = GetVersioning(migration1IsContained, migration2IsContained, migration3IsContained);

            Migrator migrator = new Migrator("", ProviderNames.SqlServer2008);
            migrator.UseCustomVersioning(versioning);
            IMigrationBatch batch = migrator.FetchMigrations(typeof(Migration1).Assembly);

            versioning.VerifyAllExpectations();
            return batch.Count;
        }

// ReSharper disable UnusedMember.Local
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private IEnumerable<TestCaseData> GetCasesForPendingMigrationsAreFound()
// ReSharper restore UnusedMember.Local
        {
            yield return new TestCaseData(true, true, true)
                .SetDescription("No pending migrations")
                .Returns(0);
            yield return new TestCaseData(true, true, false)
                .SetDescription("One pending migration at the end")
                .Returns(1);
            yield return new TestCaseData(true, false, false)
                .SetDescription("Two pending migrations at the end")
                .Returns(2);
            yield return new TestCaseData(true, false, true)
                .SetDescription("One pending migration in the middle")
                .Returns(1);
        }


        [Test]
        public void VerifyPendingMigrationsAreFoundForSpecificModule()
        {
            IVersioning versioning = GetVersioning(false, false, false);

            Migrator migrator = new Migrator("", ProviderNames.SqlServer2008, new MigrationOptions { ModuleSelector = m => m == Migration2.Module });
            migrator.UseCustomVersioning(versioning);
            IMigrationBatch batch = migrator.FetchMigrations(typeof(Migration1).Assembly);

            Assert.AreEqual(1, batch.Count, string.Format(CultureInfo.CurrentCulture, "Only one migration for the module named '{0}' exists.", Migration2.Module));
        }

        [Test, ExpectedException(typeof(IrreversibleMigrationException))]
        public void TestIrreversibleMigrationExceptionIsThrown()
        {
            long timestamp2 = typeof(Migration2).GetTimestamp();
            IVersioning versioning = GetVersioning(true, true, true);

            Migrator migrator = new Migrator("", ProviderNames.SqlServer2008);
            migrator.UseCustomVersioning(versioning);
            migrator.FetchMigrationsTo(typeof(Migration1).Assembly, timestamp2); // should throw an IrreversibleMigrationException as Migration3 is irreversible
        }

        [Test]
        public void TestCanReadFancyTimestamp()
        {
            long timestamp = typeof(M_2011_10_08_2335_AddedUserTable).GetTimestamp();
            Assert.AreEqual(201110082335, timestamp);
        }


        private static IVersioning GetVersioning(bool migration1IsContained, bool migration2IsContained, bool migration3IsContained)
        {
            IVersioning versioning = MockRepository.GenerateMock<IVersioning>();
            long timestamp1 = typeof(Migration1).GetTimestamp();
            long timestamp2 = typeof(Migration2).GetTimestamp();
            long timestamp3 = typeof(Migration3).GetTimestamp();
            versioning.Expect(v => v.IsContained(Arg<IMigrationMetadata>.Matches(m => m.Timestamp == timestamp1))).Return(migration1IsContained);
            versioning.Expect(v => v.IsContained(Arg<IMigrationMetadata>.Matches(m => m.Timestamp == timestamp2))).Return(migration2IsContained);
            versioning.Expect(v => v.IsContained(Arg<IMigrationMetadata>.Matches(m => m.Timestamp == timestamp3))).Return(migration3IsContained);
            versioning.Expect(v => v.IsContained(Arg<IMigrationMetadata>.Matches(m => m.Timestamp > timestamp3))).Return(true).Repeat.Any();
            return versioning;
        }
    }
}