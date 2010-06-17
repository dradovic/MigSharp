using System.Data;

using MigSharp.Core;
using MigSharp.Core.Entities;
using MigSharp.Providers;

using NUnit.Framework;

namespace MigSharp.NUnit
{
    [TestFixture, Category("Smoke")]
    public class SpecificationTests
    {
        [Test]
        public void TestRenamingTable()
        {
            Database db = new Database();
            db.Tables["Customers"].Rename("Customer");
            IProvider provider = new SqlServerProvider();
            CommandScripter scripter = new CommandScripter(provider);
            ScriptComparer.AssertAreEqual(
                new[] { "EXEC dbo.sp_rename @objname = N'[dbo].[Customers]', @newname = N'Customer', @objtype = N'OBJECT'" },
                scripter.GetCommandTexts(db));
        }

        [Test]
        public void TestRenamingColumn()
        {
            Database db = new Database();
            db.Tables["S_AggregatorValues"]
                .Columns["Val"].Rename("ValAbsoluteIncome");
            IProvider provider = new SqlServerProvider();
            CommandScripter scripter = new CommandScripter(provider);
            ScriptComparer.AssertAreEqual(
                new[] { "EXEC dbo.sp_rename @objname=N'[dbo].[S_AggregatorValues].[Val]', @newname=N'ValAbsoluteIncome', @objtype=N'COLUMN'" },
                scripter.GetCommandTexts(db));
        }

        [Test]
        public void Test_F518()
        {
            Database db = new Database();
            db.Tables["S_Aggregator"]
                .AddColumn("Valid Flag", DbType.Byte).WithTemporaryDefault(0)
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
            ScriptComparer.AssertAreEqual(new[]
            {
                @"ALTER TABLE [dbo].[S_Aggregator] ADD [Valid Flag] [smallint] NOT NULL CONSTRAINT [DF_S_Aggregator_Valid Flag]  DEFAULT 0",
                @"ALTER TABLE [dbo].[S_Aggregator] ADD [Paths] [int] NULL",
                @"ALTER TABLE [dbo].[S_Aggregator] ADD [PathGridpoints] [int] NULL",
                @"ALTER TABLE [dbo].[S_Aggregator] ADD [PathTimeSeries] [nvarchar](max) NULL",
                @"ALTER TABLE [dbo].[S_Aggregator] DROP CONSTRAINT [DF_S_Aggregator_Valid Flag]",
                @"CREATE TABLE [dbo].[S_EvaluatedPaths](
" + "\t" + @"[AnalysisKey] [int] NOT NULL,
" + "\t" + @"[ObjectKey] [int] NOT NULL,
" + "\t" + @"[RateCurveKey] [int] NULL,
" + "\t" + @"[Paths] [nvarchar](max) NULL,
 CONSTRAINT [PK_S_EvaluatedPaths] PRIMARY KEY " /* NONCLUSTERED FEATURE: support clustering */+ @"
(
" + "\t" + @"[AnalysisKey],
" + "\t" + @"[ObjectKey]
)WITH (IGNORE_DUP_KEY = OFF)
)
"
            },
                scripter.GetCommandTexts(db));
        }
    }
}