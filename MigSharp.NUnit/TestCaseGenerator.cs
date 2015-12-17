using System.Collections.Generic;
using System.Data;
using System.Linq;
using FakeItEasy;
using MigSharp.Core.Entities;
using MigSharp.Providers;

using NUnit.Framework;

namespace MigSharp.NUnit
{
    public static class TestCaseGenerator
    {
        public static IEnumerable<TestCaseData> CreateDatabaseCases()
        {
            IMigrationContext context = A.Fake<IMigrationContext>();

            IDatabase db = new Database(context);
            db.CreateTable("Customers")
                .WithPrimaryKeyColumn("ObjectKey", DbType.Int32)
                .WithPrimaryKeyColumn("AnalysisKey", DbType.Int32)
                .WithNullableColumn("Name", DbType.String)
                .WithNullableColumn("Name in ANSI", DbType.AnsiString)
                .WithNullableColumn("Street", DbType.String).OfSize(128)
                .WithNullableColumn("Price", DbType.Decimal).OfSize(10, 2)
                .WithNotNullableColumn("Non-nullable Column", DbType.StringFixedLength).OfSize(128)
                .WithNotNullableColumn("Unique Column", DbType.Int64).Unique()
                .WithNotNullableColumn("1st Column Under Named Unique Constraint", DbType.Int64).Unique("MyUniqueConstraint")
                .WithNotNullableColumn("2nd Column Under Named Unique Constraint", DbType.Int64).Unique("MyUniqueConstraint");
            yield return new TestCaseData(db,
                new[] { new DataType(DbType.Int32), new DataType(DbType.String), new DataType(DbType.AnsiString), new DataType(DbType.String, 128), new DataType(DbType.Decimal, 10, 2), new DataType(DbType.StringFixedLength, 128), new DataType(DbType.Int64) },
                new[] { new DataType(DbType.Int32) },
                Enumerable.Empty<DataType>(),
                "1st Column Under Named Unique Constraint")
                .SetDescription("CreateTable");

            db = new Database(context);
            db.Tables["Customers"].Drop();
            yield return new TestCaseData(db,
                Enumerable.Empty<DataType>(),
                Enumerable.Empty<DataType>(),
                Enumerable.Empty<DataType>(),
                string.Empty)
                .SetDescription("DropTable");

            db = new Database(context);
            db.Tables["Customers"]
                .AddNotNullableColumn("NewNonNullableColumn", DbType.Int32)
                .AddNotNullableColumn("NewNonNullableColumnWithTempDflt7", DbType.Int32).HavingTemporaryDefault(7)
                .AddNullableColumn("NewNullableColumn", DbType.Int64)
                .AddNotNullableColumn("MyColumn", DbType.String).OfSize(100).HavingDefault("myValue")
                .AddNotNullableColumn("MySystemTime", DbType.DateTime).HavingCurrentDateTimeAsDefault();
            yield return new TestCaseData(db,
                new[] { new DataType(DbType.Int32), new DataType(DbType.Int64), new DataType(DbType.String, 100), new DataType(DbType.DateTime) },
                Enumerable.Empty<DataType>(),
                Enumerable.Empty<DataType>(),
                "NewNonNullableColumnWithTempDflt7")
                .SetDescription("AddNotNullableColumn");

            db = new Database(context);
            db.Tables["Customers"].Columns["Some Column"].Drop();
            yield return new TestCaseData(db,
                Enumerable.Empty<DataType>(),
                Enumerable.Empty<DataType>(),
                Enumerable.Empty<DataType>(),
                string.Empty)
                .SetDescription("DropColumn");

            db = new Database(context);
            db.Tables["Customers"].Rename("Customer");
            yield return new TestCaseData(db,
                Enumerable.Empty<DataType>(),
                Enumerable.Empty<DataType>(),
                Enumerable.Empty<DataType>(),
                "Customer")
                .SetDescription("RenameTable");

            db = new Database(context);
            db.Tables["Customers"].Columns["ColumnName"].Rename("LastName");
            yield return new TestCaseData(db,
                Enumerable.Empty<DataType>(),
                Enumerable.Empty<DataType>(),
                Enumerable.Empty<DataType>(),
                "LastName")
                .SetDescription("RenameColumn");

            db = new Database(context);
            db.Tables["Orders"].AddForeignKeyTo("Customers")
                .Through("CustomerId", "Id");
            yield return new TestCaseData(db,
                Enumerable.Empty<DataType>(),
                Enumerable.Empty<DataType>(),
                Enumerable.Empty<DataType>(),
                "FK_Orders_Customers")
                .SetDescription("AddForeignKey using extension method");

            db = new Database(context);
            db.Tables["Orders"].AddForeignKeyTo("Customers", "MyFK")
                .Through("CustomerId", "Id")
                .Through("AnotherFkColumn", "Id");
            yield return new TestCaseData(db,
                Enumerable.Empty<DataType>(),
                Enumerable.Empty<DataType>(),
                Enumerable.Empty<DataType>(),
                "MyFK")
                .SetDescription("AddForeignKey with multiple columns and a custom constraint name");

            db = new Database(context);
            db.CreateTable("Table")
                .WithPrimaryKeyColumn("PK", DbType.Int32)
                .WithNotNullableColumn("Id", DbType.Decimal).OfSize(12).AsIdentity();
            yield return new TestCaseData(db,
                new[] { new DataType(DbType.Int32), new DataType(DbType.Decimal, 12) },
                new[] { new DataType(DbType.Int32) },
                new[] { new DataType(DbType.Decimal, 12) },
                "PK_Table")
                .SetDescription("Identity");

            db = new Database(context);
            db.Tables["Table"].UniqueConstraints["IX_Table_Id"].Drop();
            yield return new TestCaseData(db,
                Enumerable.Empty<DataType>(),
                Enumerable.Empty<DataType>(),
                Enumerable.Empty<DataType>(),
                string.Empty)
                .SetDescription("Drop unique constraint");

            db = new Database(context);
            db.Tables["Table"].AddUniqueConstraint("My Index")
                .OnColumn("Id")
                .OnColumn("ApplicationId");
            yield return new TestCaseData(db,
                Enumerable.Empty<DataType>(),
                Enumerable.Empty<DataType>(),
                Enumerable.Empty<DataType>(),
                "My Index")
                .SetDescription("Add unique constraint");

            db = new Database(context);
            db.Tables["Table"].AddUniqueConstraint()
                .OnColumn("Id")
                .OnColumn("ApplicationId");
            yield return new TestCaseData(db,
                Enumerable.Empty<DataType>(),
                Enumerable.Empty<DataType>(),
                Enumerable.Empty<DataType>(),
                "IX_Table_Id")
                .SetDescription("Add unique constraint with default name");

            db = new Database(context);
            db.Tables["Table"].PrimaryKey("MyPK").Drop();
            yield return new TestCaseData(db,
                Enumerable.Empty<DataType>(),
                Enumerable.Empty<DataType>(),
                Enumerable.Empty<DataType>(),
                string.Empty)
                .SetDescription("Drop primary key constraint");

            db = new Database(context);
            db.Tables["Table"].PrimaryKey().Drop();
            yield return new TestCaseData(db,
                Enumerable.Empty<DataType>(),
                Enumerable.Empty<DataType>(),
                Enumerable.Empty<DataType>(),
                string.Empty)
                .SetDescription("Drop primary key constraint with default name");

            db = new Database(context);
            db.Tables["Table"].AddPrimaryKey("MyPK")
                .OnColumn("Column1")
                .OnColumn("Column2");
            yield return new TestCaseData(db,
                Enumerable.Empty<DataType>(),
                Enumerable.Empty<DataType>(),
                Enumerable.Empty<DataType>(),
                "MyPK")
                .SetDescription("Add primary key constraint");

            db = new Database(context);
            db.Tables["Table"].ForeignKeys["MyFK"].Drop();
            yield return new TestCaseData(db,
                Enumerable.Empty<DataType>(),
                Enumerable.Empty<DataType>(),
                Enumerable.Empty<DataType>(),
                string.Empty)
                .SetDescription("Drop foreign key constraint");

            db = new Database(context);
            db.Tables["Customers"]
                .AddNotNullableColumn("MySystemTime", DbType.DateTime).HavingCurrentDateTimeAsDefault();
            yield return new TestCaseData(db,
                new[] { new DataType(DbType.DateTime) },
                Enumerable.Empty<DataType>(),
                Enumerable.Empty<DataType>(),
                "MySystemTime")
                .SetDescription("Add column with HavingCurrentDateTimeAsDefault");

            db = new Database(context);
            db.CreateTable("Customers")
                .WithNotNullableColumn("MySystemTime", DbType.DateTime).HavingCurrentDateTimeAsDefault();
            yield return new TestCaseData(db,
                new[] { new DataType(DbType.DateTime) },
                Enumerable.Empty<DataType>(),
                Enumerable.Empty<DataType>(),
                "MySystemTime")
                .SetDescription("Create column with HavingCurrentDateTimeAsDefault");

            db = new Database(context);
            db.CreateTable("Customers", "My custom PK constraint name")
                .WithPrimaryKeyColumn("Id", DbType.Int32)
                .WithNullableColumn("Some Other Column", DbType.AnsiString);
            yield return new TestCaseData(db,
                new[] { new DataType(DbType.Int32), new DataType(DbType.AnsiString) },
                new[] { new DataType(DbType.Int32) },
                Enumerable.Empty<DataType>(),
                "My custom PK constraint name")
                .SetDescription("Create table with custom primary key constraint name");

            db = new Database(context);
            db.Tables["Customers"].Indexes["MyIndex"].Drop();
            yield return new TestCaseData(db,
                Enumerable.Empty<DataType>(),
                Enumerable.Empty<DataType>(),
                Enumerable.Empty<DataType>(),
                string.Empty).SetDescription("Drop existing index");

            db = new Database(context);
            db.Tables["Customers"].AddIndex("MyIndex")
                .OnColumn("First Column")
                .OnColumn("Second Column");
            yield return new TestCaseData(db,
                Enumerable.Empty<DataType>(),
                Enumerable.Empty<DataType>(),
                Enumerable.Empty<DataType>(),
                "MyIndex")
                .SetDescription("Add an index");

            db = new Database(context);
            db.Tables["Table"].Columns["Column"].AlterToNotNullable(DbType.String);
            yield return new TestCaseData(db,
                new[] { new DataType(DbType.String) },
                Enumerable.Empty<DataType>(),
                Enumerable.Empty<DataType>(),
                string.Empty)
                .SetDescription("MakeNotNullable");

            db = new Database(context);
            db.Tables["Table"].Columns["Column"].AlterToNullable(DbType.String).OfSize(255);
            yield return new TestCaseData(db,
                new[] { new DataType(DbType.String, 255) },
                Enumerable.Empty<DataType>(),
                Enumerable.Empty<DataType>(),
                string.Empty).SetDescription("MakeNullable OfSize");

            db = new Database(context);
            db.Tables["Table"].Columns["Column"].AlterToNotNullable(DbType.String).OfSize(255).HavingDefault("my default");
            yield return new TestCaseData(db,
                new[] { new DataType(DbType.String, 255) },
                Enumerable.Empty<DataType>(),
                Enumerable.Empty<DataType>(),
                string.Empty)
                .SetDescription("MakeNullable OfSize");

            db = new Database(context);
            db.Tables["Table"].Columns["Column"].AlterToNotNullable(DbType.String).OfSize(255).HavingCurrentDateTimeAsDefault();
            yield return new TestCaseData(db,
                new[] { new DataType(DbType.String, 255) },
                Enumerable.Empty<DataType>(),
                Enumerable.Empty<DataType>(),
                string.Empty)
                .SetDescription("MakeNullable OfSize");

            db = new Database(context);
            db.Tables["Table"].PrimaryKey().Rename("New PK Name");
            yield return new TestCaseData(db,
                Enumerable.Empty<DataType>(),
                Enumerable.Empty<DataType>(),
                Enumerable.Empty<DataType>(),
                "New PK Name")
                .SetDescription("Rename Primary Key");
        }
    }
}