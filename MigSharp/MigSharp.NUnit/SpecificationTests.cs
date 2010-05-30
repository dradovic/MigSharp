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
            CollectionAssert.AreEqual(new[] { "EXEC dbo.sp_rename @objname = N'[dbo].[Customers]', @newname = N'Customer', @objtype = N'OBJECT'" }, commandTexts);
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
            CollectionAssert.AreEqual(new[] { "EXEC dbo.sp_rename @objname=N'[dbo].[S_AggregatorValues].[Val]', @newname=N'ValAbsoluteIncome', @objtype=N'COLUMN'" }, commandTexts);
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
                @"ALTER TABLE [dbo].[S_Aggregator] ADD [ValidFlag] [smallint] NOT NULL CONSTRAINT [DF_S_Aggregator_ValidFlag]  DEFAULT 0",
                @"ALTER TABLE [dbo].[S_Aggregator] ADD [Paths] [int] NULL",
                @"ALTER TABLE [dbo].[S_Aggregator] ADD [PathGridpoints] [int] NULL",
                @"ALTER TABLE [dbo].[S_Aggregator] ADD [PathTimeSeries] [nvarchar](max) NULL",
                @"ALTER TABLE [dbo].[S_Aggregator] DROP CONSTRAINT [DF_S_Aggregator_ValidFlag]",
                @"CREATE TABLE [dbo].[S_EvaluatedPaths](
" + "\t" + @"[AnalysisKey] [int] NOT NULL,
" + "\t" + @"[ObjectKey] [int] NOT NULL,
" + "\t" + @"[RateCurveKey] [int] NULL,
" + "\t" + @"[Paths] [nvarchar](max) NULL,
 CONSTRAINT [PK_S_EvaluatedPaths] PRIMARY KEY " /* NONCLUSTERED TODO: comment in*/+ @"
(
" + "\t" + @"[AnalysisKey],
" + "\t" + @"[ObjectKey]
)WITH (IGNORE_DUP_KEY = OFF)
)
"
            },
                commandTexts);
        }
    }
}