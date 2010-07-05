using System.Collections.Generic;
using System.Data;

using MigSharp.Core.Entities;

using NUnit.Framework;

using Rhino.Mocks;

namespace MigSharp.NUnit
{
    public static class TestCaseGenerator
    {
        public static IEnumerable<TestCaseData> GetDatabaseCases()
        {
            IMigrationContext context = MockRepository.GenerateStub<IMigrationContext>();

            IDatabase db = new Database(context);
            db.CreateTable("Customers")
                .WithPrimaryKeyColumn("ObjectKey", DbType.Int32)
                .WithPrimaryKeyColumn("AnalysisKey", DbType.Int32)
                .WithNullableColumn("Name", DbType.String)
                .WithNullableColumn("Street", DbType.StringFixedLength).OfLength(128);
            yield return new TestCaseData(db).SetDescription("CreateTable");

            db = new Database(context);
            db.CreateTable("Customers").IfNotExists()
                .WithPrimaryKeyColumn("ObjectKey", DbType.Int32)
                .WithPrimaryKeyColumn("AnalysisKey", DbType.Int32)
                .WithNullableColumn("Name", DbType.String)
                .WithNullableColumn("Street", DbType.StringFixedLength).OfLength(128);
            yield return new TestCaseData(db).SetDescription("CreateTable conditionally");

            db = new Database(context);
            db.Tables["Customers"].Drop();
            yield return new TestCaseData(db).SetDescription("DropTable");

            db = new Database(context);
            db.Tables["Customers"]
                .AddColumn("NewNonNullableColumn", DbType.Int32)
                .AddColumn("NewNonNullableColumnWithTempDflt7", DbType.Int32).WithTemporaryDefault(7)
                .AddNullableColumn("NewNullableColumn", DbType.Int64)
                .AddColumn("NewNonNullableColumnWithFixedLength", DbType.Int32).OfLength(128);
            yield return new TestCaseData(db).SetDescription("AddColumns");

            db = new Database(context);
            db.Tables["Customers"].Columns["Some Column"].Drop();
            yield return new TestCaseData(db).SetDescription("DropColumn");

            db = new Database(context);
            db.Tables["Customers"].Rename("Customer");
            yield return new TestCaseData(db).SetDescription("RenameTable");

            db = new Database(context);
            db.Tables["Customers"].Columns["ColumnName"].Rename("LastName");
            yield return new TestCaseData(db).SetDescription("RenameColumn");

            db = new Database(context);
            db.Tables["Customers"].Columns["ColumnName"].DropDefaultConstraint();
            yield return new TestCaseData(db).SetDescription("DropDefaultConstraint");
        }
    }
}