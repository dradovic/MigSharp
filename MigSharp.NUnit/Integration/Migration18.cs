using System.Collections.Generic;
using System.Data;
using System.Globalization;

namespace MigSharp.NUnit.Integration
{
    [MigrationExport(Tag = "Dropping Column With Default Value and Adding New Column With Default Value Having the Previous Name")]
    internal class Migration18 : IExclusiveIntegrationTestMigration
    {
        private const string FirstDefaultValue = "Test";
        private const int SecondDefaultValue = 747;

        public void Up(IDatabase db)
        {
            if (!this.IsFeatureSupported(db))
            {
                return;
            }

            var table = db.CreateTable(Tables[0].Name)
                          .WithPrimaryKeyColumn(Tables[0].Columns[0], DbType.Int32);
            table.WithNotNullableColumn(Tables[0].Columns[1], DbType.String).OfSize(10).HavingDefault(FirstDefaultValue);
            db.Execute(string.Format(CultureInfo.InvariantCulture, "INSERT INTO \"{0}\" (\"{1}\") VALUES ('{2}')", Tables[0].Name, Tables[0].Columns[0], Tables[0].Value(0, 0)));

            db.Tables[Tables[0].Name].Columns[Tables[0].Columns[1]].Drop();

            // add a new column with the same name like the previously dropped one to make sure that any associated db object were dropped, too
            db.Tables[Tables[0].Name].AddNotNullableColumn(Tables[0].Columns[1], DbType.Int32).HavingDefault(SecondDefaultValue);

            db.Execute(string.Format(CultureInfo.InvariantCulture, "INSERT INTO \"{0}\" (\"{1}\") VALUES ('{2}')", Tables[0].Name, Tables[0].Columns[0], Tables[0].Value(1, 0)));
        }

        public ExpectedTables Tables
        {
            get
            {
                return new ExpectedTables
                    {
                        new ExpectedTable("Mig18", "Id", "First")
                            {
                                { 1, SecondDefaultValue },
                                { 2, SecondDefaultValue },
                            }
                    };
            }
        }

        public IEnumerable<string> ProvidersNotSupportingFeatureUnderTest { get { return new[] { ProviderNames.SQLite }; } }
    }
}