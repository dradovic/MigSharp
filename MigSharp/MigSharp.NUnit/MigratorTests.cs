using System;
using System.Collections.Generic;

using MigSharp.NUnit.Integration;
using MigSharp.Process;

using NUnit.Framework;

using Rhino.Mocks;

namespace MigSharp.NUnit
{
    [TestFixture, Category("Smoke")]
    public class MigratorTests
    {
        [Test, TestCaseSource("GetCasesForPendingMigrationsAreFound")]
        public int VerifyPendingMigrationsAreFound(bool migration1IsContained, bool migration2IsContained, bool migration3IsContained)
        {
            IVersioning versioning = GetVersioning(migration1IsContained, migration2IsContained, migration3IsContained);

            Migrator migrator = new Migrator("", "");
            migrator.UseCustomVersioning(versioning);
            IMigrationBatch batch = migrator.FetchPendingMigrations(typeof(Migration1).Assembly);

            versioning.VerifyAllExpectations();
            return batch.Count;
        }

// ReSharper disable UnusedMember.Local
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

        // TODO: test custom bootstrapping

        [Test, ExpectedException(typeof(IrreversibleMigrationException))]
        public void TestIrreversibleMigrationExceptionIsThrown()
        {
            DateTime timestamp2 = typeof(Migration2).GetTimestamp();
            IVersioning versioning = GetVersioning(true, true, true);

            Migrator migrator = new Migrator("", "");
            migrator.UseCustomVersioning(versioning);
            migrator.FetchMigrationsTo(typeof(Migration1).Assembly, timestamp2); // should throw an IrreversibleMigrationException as Migration3 is irreversible
        }

        private IVersioning GetVersioning(bool migration1IsContained, bool migration2IsContained, bool migration3IsContained)
        {
            IVersioning versioning = MockRepository.GenerateMock<IVersioning>();
            DateTime timestamp1 = typeof(Migration1).GetTimestamp();
            DateTime timestamp2 = typeof(Migration2).GetTimestamp();
            DateTime timestamp3 = typeof(Migration3).GetTimestamp();
            versioning.Expect(v => v.IsContained(Arg<IMigrationMetadata>.Matches(m => m.Timestamp() == timestamp1))).Return(migration1IsContained);
            versioning.Expect(v => v.IsContained(Arg<IMigrationMetadata>.Matches(m => m.Timestamp() == timestamp2))).Return(migration2IsContained);
            versioning.Expect(v => v.IsContained(Arg<IMigrationMetadata>.Matches(m => m.Timestamp() == timestamp3))).Return(migration3IsContained);
            return versioning;
        }
    }
}