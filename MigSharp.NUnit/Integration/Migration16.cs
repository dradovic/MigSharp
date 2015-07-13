using System.Collections.Generic;
using System.Data;
using System.Globalization;

namespace MigSharp.NUnit.Integration
{
    [MigrationExport(Tag = "Renaming Column With Default Value and Adding New Column With Default Value Having the Previous Name")]
    internal class Migration16 : IExclusiveIntegrationTestMigration
    {
        private const string FirstDefaultValue = "Test";
        private const int SecondDefaultValue = 747;

        public void Up(IDatabase db)
        {
            if (!this.IsFeatureSupported(db))
            {
                return;
            }

            db.CreateTable(Tables[0].Name)
              .WithPrimaryKeyColumn(Tables[0].Columns[0], DbType.Int32)
              .WithNotNullableColumn(Tables[0].Columns[2], DbType.String).OfSize(10).HavingDefault(FirstDefaultValue);

            db.Execute(string.Format(CultureInfo.InvariantCulture, "INSERT INTO \"{0}\" (\"{1}\") VALUES ('{2}')", Tables[0].Name, Tables[0].Columns[0], Tables[0].Value(0, 0)));

            db.Tables[Tables[0].Name].Columns[Tables[0].Columns[2]].Rename(Tables[0].Columns[1]);

            // add a new column with the same name like the previously renamed one to make sure that any associated db object were renamed, too
            db.Tables[Tables[0].Name].AddNotNullableColumn(Tables[0].Columns[2], DbType.Int32).HavingDefault(SecondDefaultValue);

            db.Execute(string.Format(CultureInfo.InvariantCulture, "INSERT INTO \"{0}\" (\"{1}\") VALUES ('{2}')", Tables[0].Name, Tables[0].Columns[0], Tables[0].Value(1, 0)));
        }

        public ExpectedTables Tables
        {
            get
            {
                return new ExpectedTables
                    {
                        new ExpectedTable("Mig16", "Id", "First Renamed", "First")
                            {
                                { 1, FirstDefaultValue, SecondDefaultValue },
                                { 2, FirstDefaultValue, SecondDefaultValue },
                            }
                    };
            }
        }

        public IEnumerable<string> ProvidersNotSupportingFeatureUnderTest
        {
            get
            {
                return new[]
                    {
                        ProviderNames.MySql,
                        ProviderNames.SqlServerCe35,
                        ProviderNames.SqlServerCe4,
                        ProviderNames.SQLite,
                    };
            }
        }
    }
}