using System.Collections.Generic;
using System.Data;

using MigSharp.Core;
using MigSharp.Providers;

using NUnit.Framework;

namespace MigSharp.NUnit
{
    [TestFixture]
    public class SpecificationTests
    {
        [Test]
        public void TestRenamingTable()
        {
            Database db = new Database();
            db.Tables["Customers"].Rename("Customer");
            IProvider provider = new SqlServerProvider();
            CommandScripter scripter = new CommandScripter(provider);
            List<string> commandTexts = new List<string>(scripter.GetCommandTexts(db));
            CollectionAssert.AreEqual(new[] { "sp_rename N'Customers', N'Customer'" }, commandTexts);
        }

        [Test]
        public void TestRenamingColumn()
        {
            Database db = new Database();
            db.Tables["S_AggregatorValues"]
                .Columns["Val"].Rename("ValAbsoluteIncome");
            IProvider provider = new SqlServerProvider();
            CommandScripter scripter = new CommandScripter(provider);
            List<string> commandTexts = new List<string>(scripter.GetCommandTexts(db));
            CollectionAssert.AreEqual(new[] { "sp_rename N'Val', N'ValAbsoluteIncome', 'COLUMN'" }, commandTexts);
        }

        [Test]
        public void Test_F518()
        {
            Database db = new Database();
            db.Tables["S_Aggregator"]
                .AddColumn("ValidFlag", DbType.Byte, 0, AddColumnOptions.DropDefaultAfterCreation)
                .AddNullableColumn("Paths", DbType.Int32)
                .AddNullableColumn("PathGridpoints", DbType.Int32)
                .AddNullableColumn("PathTimeSeries", DbType.String);
            db.CreateTable("S_EvaluatedPaths")
                .WithPrimaryKeyColumn("AnalysisKey", DbType.Int32)
                .WithPrimaryKeyColumn("ObjectKey", DbType.Int32)
                .WithNullableColumn("RateCurveKey", DbType.Int32)
                .WithNullableColumn("Paths", DbType.String);
            IProvider provider = new SqlServerProvider();
            CommandScripter scripter = new CommandScripter(provider);
            List<string> commandTexts = new List<string>(scripter.GetCommandTexts(db));
            CollectionAssert.AreEqual(new[]
            {
                @"ALTER TABLE dbo.[S_Aggregator] ADD
  [ValidFlag] SMALLINT NOT NULL CONSTRAINT DF_S_Aggregator_ValidFlag DEFAULT 0,
  [Paths] INT NULL,
  [PathGridpoints] INT NULL,
  [PathTimeSeries] NVARCHAR(MAX) NULL",
                @"ALTER TABLE dbo.[S_Aggregator] DROP CONSTRAINT DF_S_Aggregator_ValidFlag",
                @"CREATE TABLE dbo.[S_EvaluatedPaths] (
  [AnalysisKey] INT NOT NULL,
  [ObjectKey] INT NOT NULL,
  [RateCurveKey] INT NULL,
  [Paths] NVARCHAR(MAX) NULL
  CONSTRAINT PK_S_EvaluatedPaths PRIMARY KEY"/* NONCLUSTERED TODO: comment int*/+ @" (
    [AnalysisKey] ASC,
    [ObjectKey] ASC
  ) ON [PRIMARY]
) ON [PRIMARY]"
            },
                commandTexts);
        }
    }
}