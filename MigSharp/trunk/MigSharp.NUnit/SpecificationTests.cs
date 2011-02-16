using System.Data;

using MigSharp.Core;
using MigSharp.Core.Entities;
using MigSharp.Providers;

using NUnit.Framework;

using Rhino.Mocks;

namespace MigSharp.NUnit
{
    [TestFixture, Category("smoke")]
    public class SpecificationTests
    {
        [Test]
        public void TestRenamingTable()
        {
            IMigrationContext context = MockRepository.GenerateStub<IMigrationContext>();
            Database db = new Database(context);
            db.Tables["Customers"].Rename("Customer");
            IProvider provider = new SqlServer2005Provider();
            CommandScripter scripter = new CommandScripter(provider);
            ScriptComparer.AssertAreEqual(
                new[] { "EXEC dbo.sp_rename @objname = N'[dbo].[Customers]', @newname = N'Customer', @objtype = N'OBJECT'" },
                scripter.GetCommandTexts(db, MockRepository.GenerateStub<IRuntimeContext>()));
        }

        [Test]
        public void TestRenamingColumn()
        {
            IMigrationContext context = MockRepository.GenerateStub<IMigrationContext>();
            Database db = new Database(context);
            db.Tables["S_AggregatorValues"]
                .Columns["Val"].Rename("ValAbsoluteIncome");
            IProvider provider = new SqlServer2005Provider();
            CommandScripter scripter = new CommandScripter(provider);
            ScriptComparer.AssertAreEqual(
                new[] { "EXEC dbo.sp_rename @objname=N'[dbo].[S_AggregatorValues].[Val]', @newname=N'ValAbsoluteIncome', @objtype=N'COLUMN'" },
                scripter.GetCommandTexts(db, MockRepository.GenerateStub<IRuntimeContext>()));
        }

        [Test]
        public void TestF518()
        {
            IMigrationContext context = MockRepository.GenerateStub<IMigrationContext>();
            Database db = new Database(context);
            db.Tables["S_Aggregator"]
                .AddNotNullableColumn("Valid Flag", DbType.Byte).HavingTemporaryDefault(0)
                .AddNullableColumn("Paths", DbType.Int32)
                .AddNullableColumn("PathGridpoints", DbType.Int32)
                .AddNullableColumn("PathTimeSeries", DbType.String);
            db.CreateTable("S_EvaluatedPaths")
                .WithPrimaryKeyColumn("AnalysisKey", DbType.Int32)
                .WithPrimaryKeyColumn("ObjectKey", DbType.Int32)
                .WithNotNullableColumn("RateCurveKey", DbType.Int32)
                .WithNotNullableColumn("Paths", DbType.String);
            IProvider provider = new SqlServer2005Provider();
            CommandScripter scripter = new CommandScripter(provider);
            ScriptComparer.AssertAreEqual(new[]
            {
                @"ALTER TABLE [dbo].[S_Aggregator] ADD [Valid Flag] [smallint] NOT NULL CONSTRAINT [S_Aggregator_Valid Flag_DF]  DEFAULT 0",
                @"ALTER TABLE [dbo].[S_Aggregator] DROP CONSTRAINT [S_Aggregator_Valid Flag_DF]",
                @"ALTER TABLE [dbo].[S_Aggregator] ADD [Paths] [int] NULL",
                @"ALTER TABLE [dbo].[S_Aggregator] ADD [PathGridpoints] [int] NULL",
                @"ALTER TABLE [dbo].[S_Aggregator] ADD [PathTimeSeries] [nvarchar](max) NULL",
                @"CREATE TABLE [dbo].[S_EvaluatedPaths](
" + "\t" + @"[AnalysisKey] [int] NOT NULL,
" + "\t" + @"[ObjectKey] [int] NOT NULL,
" + "\t" + @"[RateCurveKey] [int] NOT NULL,
" + "\t" + @"[Paths] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_S_EvaluatedPaths] PRIMARY KEY " /* NONCLUSTERED FEATURE: support clustering */+ @"
(
" + "\t" + @"[AnalysisKey],
" + "\t" + @"[ObjectKey]
)WITH (IGNORE_DUP_KEY = OFF)
)
"
            },
                scripter.GetCommandTexts(db, MockRepository.GenerateStub<IRuntimeContext>()));
        }
    }
}