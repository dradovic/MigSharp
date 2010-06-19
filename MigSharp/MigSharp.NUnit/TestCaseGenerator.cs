using System.Collections.Generic;
using System.Data;

using MigSharp.Core.Entities;

using NUnit.Framework;

namespace MigSharp.NUnit
{
    public static class TestCaseGenerator
    {
        public static IEnumerable<TestCaseData> GetDatabaseCases()
        {
            IDatabase db = new Database();
            db.CreateTable("Customers")
                .WithPrimaryKeyColumn("ObjectKey", DbType.Int32)
                .WithPrimaryKeyColumn("AnalysisKey", DbType.Int32)
                .WithNullableColumn("Name", DbType.String)
                .WithNullableColumn("Street", DbType.StringFixedLength).OfLength(128);
            yield return new TestCaseData(db).SetDescription("CreateTable");

            db = new Database();
            db.CreateTable("Customers").IfNotExists()
                .WithPrimaryKeyColumn("ObjectKey", DbType.Int32)
                .WithPrimaryKeyColumn("AnalysisKey", DbType.Int32)
                .WithNullableColumn("Name", DbType.String)
                .WithNullableColumn("Street", DbType.StringFixedLength).OfLength(128);
            yield return new TestCaseData(db).SetDescription("CreateTable conditionally");

            db = new Database();
            db.Tables["Customers"].Drop();
            yield return new TestCaseData(db).SetDescription("DropTable");

            db = new Database();
            db.Tables["Customers"]
                .AddColumn("NewNonNullableColumn", DbType.Int32)
                .AddColumn("NewNonNullableColumnWithTempDflt7", DbType.Int32).WithTemporaryDefault(7)
                .AddNullableColumn("NewNullableColumn", DbType.Int32)
                .AddColumn("NewNonNullableColumnWithFixedLength", DbType.Int32).OfLength(128);
            yield return new TestCaseData(db).SetDescription("AddColumns");

            db = new Database();
            db.Tables["Customers"].Columns["Some Column"].Drop();
            yield return new TestCaseData(db).SetDescription("DropColumn");

            db = new Database();
            db.Tables["Customers"].Rename("Customer");
            yield return new TestCaseData(db).SetDescription("RenameTable");

            db = new Database();
            db.Tables["Customers"].Columns["ColumnName"].Rename("LastName");
            yield return new TestCaseData(db).SetDescription("RenameColumn");

            db = new Database();
            db.Tables["Customers"].Columns["ColumnName"].DropDefaultConstraint();
            yield return new TestCaseData(db).SetDescription("DropDefaultConstraint");
        }
    }
}