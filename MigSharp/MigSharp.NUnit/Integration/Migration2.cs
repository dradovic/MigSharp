using System;
using System.Data;

namespace MigSharp.NUnit.Integration
{
    [MigrationExport(2010, 6, 17, 12, 50, 21, ModuleName = Module, Tag = Tag)]
    internal class Migration2 : IUndoableMigration
    {
        public const string Module = "Migration 2";
        public const string Tag = "Some very informative tag...";

        public const string OrderTableName = "Order Space";
        public static string[] ColumnNames = new[]
        {
            "Id Space",
        };
        public const int FirstId = 1;

        public void Up(IDatabase db)
        {
            db.Execute("bogus query which would fail").IfUsing("a provider that does not exist");

            db.CreateTable(OrderTableName)
                .WithPrimaryKeyColumn(ColumnNames[0], DbType.Int32);

            db.Execute(string.Format("INSERT INTO [{0}] VALUES (1)", OrderTableName, FirstId));
        }

        public void Down(IDatabase db)
        {
            db.Tables[OrderTableName].Drop();
        }
    }
}