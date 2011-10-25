//using System.Collections.Generic;
//using System.Diagnostics.CodeAnalysis;
//using System.Globalization;

//using MigSharp.NUnit.Integration;
//using MigSharp.Process;

//using NUnit.Framework;

//using Rhino.Mocks;

//using System.Linq;

//namespace MigSharp.NUnit
//{
//    [TestFixture, Category("smoke")]
//    public class MigratorTests // FIXME: dr, test UnidentifiedMigrations, too
//    {
//        // FIXME: dr, refactor and reactivate
//        //[Test, TestCaseSource("GetCasesForPendingMigrationsAreFound")]
//        //public int VerifyPendingMigrationsAreFound(bool migration1IsContained, bool migration2IsContained, bool migration3IsContained)
//        //{
//        //    IVersioning versioning = GetVersioning(migration1IsContained, migration2IsContained, migration3IsContained);

//        //    Migrator migrator = new Migrator("", ProviderNames.SqlServer2008);
//        //    migrator.UseCustomVersioning(versioning);
//        //    IMigrationBatch batch = migrator.FetchMigrations(typeof(Migration1).Assembly);

//        //    versioning.VerifyAllExpectations();
//        //    return batch.ScheduledMigrations.Count();
//        //}

//// ReSharper disable UnusedMember.Local
//        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
//        private IEnumerable<TestCaseData> GetCasesForPendingMigrationsAreFound()
//// ReSharper restore UnusedMember.Local
//        {
//            yield return new TestCaseData(true, true, true)
//                .SetDescription("No pending migrations")
//                .Returns(0);
//            //yield return new TestCaseData(true, true, false)
//            //    .SetDescription("One pending migration at the end")
//            //    .Returns(1);
//            //yield return new TestCaseData(true, false, false)
//            //    .SetDescription("Two pending migrations at the end")
//            //    .Returns(2);
//            //yield return new TestCaseData(true, false, true)
//            //    .SetDescription("One pending migration in the middle")
//            //    .Returns(1);
//        }

//        //[Test]
//        //public void VerifyPendingMigrationsAreFoundForSpecificModule()
//        //{
//        //    IVersioning versioning = GetVersioning(false, false, false);

//        //    Migrator migrator = new Migrator("", ProviderNames.SqlServer2008, new MigrationOptions { ModuleSelector = m => m == Migration2.Module });
//        //    migrator.UseCustomVersioning(versioning);
//        //    IMigrationBatch batch = migrator.FetchMigrations(typeof(Migration1).Assembly);

//        //    Assert.AreEqual(1, batch.ScheduledMigrations.Count(), string.Format(CultureInfo.CurrentCulture, "Only one migration for the module named '{0}' exists.", Migration2.Module));
//        //}

//        //[Test, ExpectedException(typeof(IrreversibleMigrationException))]
//        //public void TestIrreversibleMigrationExceptionIsThrown()
//        //{
//        //    long timestamp2 = typeof(Migration2).GetTimestamp();
//        //    IVersioning versioning = GetVersioning(true, true, true);

//        //    Migrator migrator = new Migrator("", ProviderNames.SqlServer2008);
//        //    migrator.UseCustomVersioning(versioning);
//        //    migrator.FetchMigrationsTo(typeof(Migration1).Assembly, timestamp2); // should throw an IrreversibleMigrationException as Migration3 is irreversible
//        //}


//        //private static IVersioning GetVersioning(bool migration1IsContained, bool migration2IsContained, bool migration3IsContained)
//        //{
//        //    IVersioning versioning = MockRepository.GenerateMock<IVersioning>();
//        //    long timestamp1 = typeof(Migration1).GetTimestamp();
//        //    long timestamp2 = typeof(Migration2).GetTimestamp();
//        //    long timestamp3 = typeof(Migration3).GetTimestamp();
//        //    List<MigrationInfo> migrations = new List<MigrationInfo>();
//        //    if (migration1IsContained)
//        //    {
//        //        migrations.Add(new MigrationInfo(timestamp1, MigrationExportAttribute.DefaultModuleName, string.Empty));
//        //    }
//        //    if (migration2IsContained)
//        //    {
//        //        migrations.Add(new MigrationInfo(timestamp2, Migration2.Module, string.Empty));
//        //    }
//        //    if (migration3IsContained)
//        //    {
//        //        migrations.Add(new MigrationInfo(timestamp3, MigrationExportAttribute.DefaultModuleName, string.Empty));
//        //    }
//        //    foreach (long timestamp in IntegrationTestsBase.Timestamps.Skip(3))
//        //    {
//        //        migrations.Add(new MigrationInfo(timestamp, MigrationExportAttribute.DefaultModuleName, string.Empty));
//        //    }
//        //    versioning.Expect(v => v.ExecutedMigrations).Return(migrations);
//        //    return versioning;
//        //}
//    }
//}