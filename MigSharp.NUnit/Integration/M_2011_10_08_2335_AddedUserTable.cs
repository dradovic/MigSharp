using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace MigSharp.NUnit.Integration
{
    [MigrationExport]
    internal class M_2011_10_08_2335_AddedUserTable : IIntegrationTestMigration
    {
        public void Up(IDatabase db)
        {
            db.CreateTable(Tables[0].Name)
                .WithPrimaryKeyColumn(Tables[0].Columns[0], DbType.Int32);
        }

        public ExpectedTables Tables
        {
            get
            {
                return new ExpectedTables
                {
                    new ExpectedTable("Mig_2011_10_08_2335_users", "Id")
                };
            }
        }
    }
}
