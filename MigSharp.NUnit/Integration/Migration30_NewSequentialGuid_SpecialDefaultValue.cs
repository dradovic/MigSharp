using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using NUnit.Framework;

namespace MigSharp.NUnit.Integration
{
    [MigrationExport(Tag = "Test using SpecialDefaultValue.NewGuid and SpecialDefaultValue.NewSequentialGuid")]
    internal class Migration30 : IExclusiveIntegrationTestMigration
    {
        private Guid _lastCheckedId;

        public void Up(IDatabase db)
        {
            if (!this.IsFeatureSupported(db))
            {
                return;
            }

            db.CreateTable(Tables[0].Name)
                .WithPrimaryKeyColumn(Tables[0].Columns[0], DbType.Guid).HavingNewSequentialGuidAsDefault()
                .WithNotNullableColumn(Tables[0].Columns[1], DbType.Guid).HavingNewGuidAsDefault()
                .WithNotNullableColumn(Tables[0].Columns[2], DbType.String);

            db.Execute(string.Format(CultureInfo.InvariantCulture, @"INSERT INTO ""{0}"" (""{1}"") VALUES ('{2}')", Tables[0].Name, Tables[0].Columns[2], Tables[0].Value(0, 2)));
            db.Execute(string.Format(CultureInfo.InvariantCulture, @"INSERT INTO ""{0}"" (""{1}"") VALUES ('{2}')", Tables[0].Name, Tables[0].Columns[2], Tables[0].Value(1, 2)));
        }

        private bool AssertGuid(object value)
        {
            Guid id;
            if (value is Guid) // SQL Server
            {
                id = (Guid)value;
            }
            else // Oracle
            {
                id = new Guid((byte[])value);
            }
            Assert.AreNotEqual(_lastCheckedId, id);
            _lastCheckedId = id;
            return id != Guid.Empty;
        }

        public ExpectedTables Tables
        {
            get
            {
                return new ExpectedTables
                {
                    new ExpectedTable("Mig30", "Id", "OtherId", "Content")
                    {
                        {
                            new Func<object, bool>(AssertGuid), new Func<object, bool>(AssertGuid), "First Row"
                        },
                        {
                            new Func<object, bool>(AssertGuid), new Func<object, bool>(AssertGuid), "Second Row"
                        },
                    }
                };
            }
        }

        public IEnumerable<Platform> PlatformsNotSupportingFeatureUnderTest
        {
            get
            {
                return new[]
                {
                        Platform.MySql,
                        Platform.Teradata,
                        Platform.SQLite
                };
            }
        }
    }
}