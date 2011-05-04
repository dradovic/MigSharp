using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;

using MigSharp.Core.Entities;
using MigSharp.Process;
using MigSharp.Providers;

using NUnit.Framework;

using Rhino.Mocks;

using System.Linq;

namespace MigSharp.NUnit.Process
{
    [TestFixture, Category("smoke")]
    public class MigrationReportTests
    {
        [Test, TestCaseSource(typeof(TestCaseGenerator), "CreateDatabaseCases")]
        public void VerifyMigrationReportProps(IDatabase database, IEnumerable<DataType> expectedDataTypes, IEnumerable<DataType> expectedPrimaryKeyDataTypes, string expectedLongestName)
        {
            const string migrationName = "Test Migration";
            MigrationReport report = MigrationReport.Create((Database)database, migrationName);
            Assert.AreEqual(migrationName, report.MigrationName);
            Assert.IsEmpty(report.Error, "These cases should not have any errors.");
            CollectionAssert.AreEquivalent(expectedDataTypes.ToList(), report.DataTypes.ToList(), "The collection of used data types is wrong.");
            CollectionAssert.AreEquivalent(expectedPrimaryKeyDataTypes.ToList(), report.PrimaryKeyDataTypes.ToList(), "The collection of used data types for primary keys is wrong.");
            Assert.AreEqual(expectedLongestName, report.LongestName, "The longest name is wrong.");
        }

        [Test, TestCaseSource("GetInvalidDatabaseCases")]
        public void VerifyValidationError(IDatabase database, string expectedError, string expectedLongestName)
        {
            MigrationReport report = MigrationReport.Create((Database)database, string.Empty);
            Assert.AreEqual(expectedError, report.Error, "The error is wrong.");
            Assert.AreEqual(expectedLongestName, report.LongestName, "The longest name is wrong.");
        }

// ReSharper disable UnusedMember.Local
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private static IEnumerable<TestCaseData> GetInvalidDatabaseCases() // called by VerifyValidationError
// ReSharper restore UnusedMember.Local
        {
            IMigrationContext context = MockRepository.GenerateStub<IMigrationContext>();

            IDatabase db = new Database(context);
            db.CreateTable("Customers");
            yield return new TestCaseData(db, "At least one column must be added to the CreateTable command.", "").SetDescription("CreateTable");

            db = new Database(context);
            db.Tables["Customers"].AddForeignKeyTo("Orders");
            yield return new TestCaseData(db, "At least one column must be added to the AddForeignKeyTo command.", "").SetDescription("AddForeignKeyTo");

            db = new Database(context);
            db.Tables["Customers"].AddUniqueConstraint();
            yield return new TestCaseData(db, "At least one column must be added to the AddUniqueConstraint command.", "").SetDescription("AddUniqueConstraint");

            db = new Database(context);
            db.Tables["Customers"].AddPrimaryKey();
            yield return new TestCaseData(db, "At least one column must be added to the AddPrimaryKey command.", "").SetDescription("AddPrimaryKey");

            db = new Database(context);
            db.CreateTable("Customers")
                .WithNotNullableColumn("Name", DbType.String).Unique("MyVeryLongUniqeConstraintName");
            yield return new TestCaseData(db, "", "MyVeryLongUniqeConstraintName").SetDescription("CreateTable with long unique constraint name");

            db = new Database(context);
            db.Tables["Customers"]
                .AddNullableColumn("Name", DbType.String).HavingDefault("Unicorn");
            yield return new TestCaseData(db, "Adding nullable columns with default values is not supported: some database platforms (like SQL Server) leave missing values NULL and some update missing values to the default value. Consider adding the column first as not-nullable, and then altering it to nullable.", "").SetDescription("Default values nullable columns");

            db = new Database(context);
            db.Tables["Customers"]
                .AddNullableColumn("Name", DbType.String).HavingTemporaryDefault("Unicorn");
            yield return new TestCaseData(db, "Adding nullable columns with default values is not supported: some database platforms (like SQL Server) leave missing values NULL and some update missing values to the default value. Consider adding the column first as not-nullable, and then altering it to nullable.", "").SetDescription("Default values nullable columns");

            db = new Database(context);
            db.CreateTable("Customers")
                .WithNotNullableColumn("Name", DbType.Double).AsIdentity();
            yield return new TestCaseData(db, "Identity is only allowed on Int32 and Int64 typed columns.", "").SetDescription("Identity on invalid data type");
        }
    }
}