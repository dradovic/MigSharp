using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using FakeItEasy;
using MigSharp.Core.Entities;
using MigSharp.Process;
using MigSharp.Providers;

using NUnit.Framework;

using System.Linq;

namespace MigSharp.NUnit.Process
{
    [TestFixture, Category("smoke")]
    internal class MigrationReportTests
    {
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [Test, TestCaseSource(typeof(TestCaseGenerator), "CreateDatabaseCases")]
        public void VerifyMigrationReportProps(IDatabase database, 
            IEnumerable<DataType> expectedDataTypes,
            IEnumerable<DataType> expectedPrimaryKeyDataTypes, 
            IEnumerable<DataType> expectedIdentityDataTypes,
            string expectedLongestName)
        {
            const string migrationName = "Test Migration";
            MigrationReport report = MigrationReport.Create((Database)database, migrationName, A.Fake<IMigrationContext>());
            Assert.AreEqual(migrationName, report.MigrationName);
            Assert.IsEmpty(report.Error, "These cases should not have any errors.");
            CollectionAssert.AreEquivalent(expectedDataTypes.ToList(), report.DataTypes.ToList(), "The collection of used data types is wrong.");
            CollectionAssert.AreEquivalent(expectedPrimaryKeyDataTypes.ToList(), report.PrimaryKeyDataTypes.ToList(), "The collection of used data types for primary keys is wrong.");
            CollectionAssert.AreEquivalent(expectedIdentityDataTypes.ToList(), report.IdentityDataTypes.ToList(), "The collection of used data types for identity columns is wrong.");
            Assert.AreEqual(expectedLongestName, report.LongestName, "The longest name is wrong.");
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        [Test, TestCaseSource("GetInvalidDatabaseCases")]
        public void VerifyValidationError(IDatabase database, string expectedError, string expectedLongestName)
        {
            MigrationReport report = MigrationReport.Create((Database)database, string.Empty, A.Fake<IMigrationContext>());
            Assert.AreEqual(expectedError, report.Error, "The error is wrong.");
            Assert.AreEqual(expectedLongestName, report.LongestName, "The longest name is wrong.");
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
// ReSharper disable UnusedMethodReturnValue.Local
        private static IEnumerable<TestCaseData> GetInvalidDatabaseCases() // called by VerifyValidationError
// ReSharper restore UnusedMethodReturnValue.Local
        {
            IMigrationContext context = A.Fake<IMigrationContext>();

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
        }
    }
}