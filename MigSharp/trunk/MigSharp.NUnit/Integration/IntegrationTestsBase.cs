using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

using MigSharp.Process;
using MigSharp.Providers;

using NUnit.Framework;

using Rhino.Mocks;

namespace MigSharp.NUnit.Integration
{
    public abstract class IntegrationTestsBase
    {
        private static readonly List<IMigration> Migrations = new List<IMigration>();

        private static readonly IList<long> Timestamps;

        private MigrationOptions _options;

        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static IntegrationTestsBase()
        {
            const string pattern = @"Migration(\d+)";
            Migrations.AddRange(typeof(IntegrationTestsBase).Assembly.GetTypes()
                .Where(t => Regex.IsMatch(t.Name, pattern))
                .OrderBy(t => int.Parse(Regex.Match(t.Name, pattern).Groups[1].Value, CultureInfo.InvariantCulture))
                .Select(t => (IMigration)Activator.CreateInstance(t)));
            Timestamps = new List<long>(Migrations.Select(m => m.GetType().GetTimestamp()));
            MigrationOptions.SetGeneralTraceLevel(SourceLevels.All);
            MigrationOptions.SetPerformanceTraceLevel(SourceLevels.All);
            MigrationOptions.SetSqlTraceLevel(SourceLevels.All);
        }

        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "TestIs")]
        [Test]
        public void TestIsUpToDate()
        {
            _options.VersioningTableName = "My Versioning Table"; // test overriding the default versioning table name
            var migrator = new Migrator(ConnectionString, ProviderName, _options);
            Assert.IsFalse(migrator.IsUpToDate(typeof(Migration1).Assembly));

            DataTable versioningTable = GetTable(_options.VersioningTableName);
            Assert.IsNull(versioningTable, "Migrator.IsUpToDate should not have any side-effects. In particualar, it should *not* create a versioning table. This allows for being able to check the up-to-dateness of a db without having the privilege to create tables.");
        }

        [Test]
        public void TestMigration1()
        {
            _options.VersioningTableName = "My Versioning Table"; // test overriding the default versioning table name
            var migrator = new Migrator(ConnectionString, ProviderName, _options);
            migrator.MigrateTo(typeof(Migration1).Assembly, Timestamps[0]);

            // assert Versioning table was created
            DataTable versioningTable = GetTable(_options.VersioningTableName);
            Assert.IsNotNull(versioningTable, string.Format(CultureInfo.CurrentCulture, "The '{0}' table was not created.", _options.VersioningTableName));
            Assert.AreEqual(3, versioningTable.Columns.Count);
            Assert.AreEqual(BootstrapMigration.TimestampColumnName, versioningTable.Columns[0].ColumnName);
            Assert.AreEqual(BootstrapMigration.ModuleColumnName, versioningTable.Columns[1].ColumnName);
            Assert.AreEqual(BootstrapMigration.TagColumnName, versioningTable.Columns[2].ColumnName);

            // assert Customer table was created
            Migration1 migration1 = new Migration1();
            DataTable customerTable = GetTable(migration1.TableName);
            Assert.IsNotNull(customerTable, string.Format(CultureInfo.CurrentCulture, "The '{0}' table was not created.", migration1.TableName));
            Assert.AreEqual(1, customerTable.Columns.Count);
            Assert.AreEqual(migration1.ColumnNames[0], customerTable.Columns[0].ColumnName);

            // assert Versioning table has necessary entries
            Assert.AreEqual(1, versioningTable.Rows.Count, "The versioning table is missing entries.");
            Assert.AreEqual(Timestamps[0], versioningTable.Rows[0][0], "The timestamp of Migration1 is wrong.");
            Assert.AreEqual(MigrationExportAttribute.DefaultModuleName, versioningTable.Rows[0][1], "The module of Migration1 is wrong.");
            Assert.AreEqual(DBNull.Value, versioningTable.Rows[0][2], "The tag of Migration1 is wrong.");
        }

        [Test]
        public void TestMigration1SucceededByAllOtherMigrations()
        {
            // initialize special-case migrations with additional runtime data
            IProviderMetadata providerMetadata;
            IProvider provider = _options.SupportedProviders.GetProvider(ProviderName, out providerMetadata);
            Migration5.Initialize(provider.GetSupportsAttributes());
            Migration8.Initialize(provider.GetSupportsAttributes());

            // execute Migration1
            var migrator = new Migrator(ConnectionString, ProviderName, _options);
            Assembly assemblyContainingMigrations = typeof(Migration1).Assembly;
            migrator.MigrateTo(assemblyContainingMigrations, Timestamps[0]);

            // execute all other migrations
            migrator = new Migrator(ConnectionString, ProviderName, _options);
            migrator.MigrateAll(assemblyContainingMigrations);
            Assert.IsTrue(migrator.IsUpToDate(assemblyContainingMigrations));

            // assert all tables have been created with the expected content
            foreach (IIntegrationTestMigration migration in Migrations.OfType<IIntegrationTestMigration>())
            {
                DataTable table = GetTable(migration.TableName);

                Assert.IsNotNull(table, string.Format(CultureInfo.CurrentCulture, "The '{0}' table was not created.", migration.TableName));
                Assert.AreEqual(migration.ColumnNames.Length, table.Columns.Count, "The actual number of columns is wrong.");
                Assert.AreEqual(migration.ExpectedValues.GetLength(0), table.Rows.Count, "The actual number of rows is wrong.");
                for (int column = 0; column < migration.ColumnNames.Length; column++)
                {
                    for (int row = 0; row < migration.ExpectedValues.GetLength(0); row++)
                    {
                        object expectedValue = migration.ExpectedValues[row, column];
                        object actualValue = table.Rows[row][column];
                        Func<object, bool> evalValue = expectedValue as Func<object, bool>;
                        if (evalValue != null)
                        {
                            Assert.IsTrue(evalValue(actualValue), string.Format(CultureInfo.CurrentCulture, "In {0}, the actual value of cell {1}/{2} is wrong (the custom handler returned false).", migration.TableName, row, column));
                        }
                        else
                        {
                            Assert.AreEqual(expectedValue, actualValue, string.Format(CultureInfo.CurrentCulture, "In {0}, the actual value of cell {1}/{2} is wrong.", migration.TableName, row, column));
                        }
                    }
                }
            }

            // assert Versioning table has necessary entries
            DataTable versioningTable = GetTable(_options.VersioningTableName);
            Assert.AreEqual(Migrations.Count, versioningTable.Rows.Count, "The versioning table has a wrong number of entries.");
            DataRow[] versioningRows = versioningTable.Select(null, BootstrapMigration.TimestampColumnName); // order by Timestamp
            for (int i = 0; i < Migrations.Count; i++)
            {
                Assert.AreEqual(Timestamps[i], versioningRows[i][0], string.Format(CultureInfo.CurrentCulture, "The timestamp of Migration{0} is wrong.", i + 1));
            }

            // check Module and Tag of Migration2 (special case)
            Assert.AreEqual(Migration2.Module, versioningRows[1][1], "The module of Migration2 is wrong.");
            Assert.AreEqual(Migration2.Tag, versioningRows[1][2], "The tag of Migration2 is wrong.");
        }

        [Test]
        public void TestUndoingMigration2()
        {
            var migrator = new Migrator(ConnectionString, ProviderName, _options);
            Assembly assemblyContainingMigrations = typeof(Migration1).Assembly;
            migrator.MigrateTo(assemblyContainingMigrations, Timestamps[1]);

            migrator = new Migrator(ConnectionString, ProviderName);
            migrator.MigrateTo(assemblyContainingMigrations, Timestamps[0]);

            // assert order table was dropped
            DataTable orderTable = GetTable(new Migration2().TableName);
            Assert.IsNull(orderTable, "The order table was not dropped.");

            // assert Versioning table has only necessary entries
            DataTable versioningTable = GetTable(_options.VersioningTableName);
            Assert.AreEqual(1, versioningTable.Rows.Count, "The versioning table is missing entries or has too much entries.");
            Assert.AreEqual(Timestamps[0], versioningTable.Rows[0][0], "The timestamp of Migration1 is wrong.");
            Assert.AreEqual(MigrationExportAttribute.DefaultModuleName, versioningTable.Rows[0][1], "The module of Migration1 is wrong.");
            Assert.AreEqual(DBNull.Value, versioningTable.Rows[0][2], "The tag of Migration1 is wrong.");
        }

        [Test]
        public void TestCustomBootstrapping()
        {
            // use a Module selection to verify that the bootstrapping is still considering *all* migrations
            _options.ModuleSelector = moduleName => moduleName == Migration2.Module;
            Migrator migrator = new Migrator(ConnectionString, ProviderName, _options);

            IBootstrapper bootstrapper = MockRepository.GenerateStrictMock<IBootstrapper>();
            bootstrapper.Expect(b => b.BeginBootstrapping(null, null)).IgnoreArguments();
            for (int i = 0; i < Migrations.Count; i++) // assume that all migrations were already performed
            {
                long timestamp = Timestamps[i];
                bootstrapper.Expect(b => b.IsContained(Arg<IMigrationMetadata>.Matches(m => m.Timestamp == timestamp))).Return(true);
            }
            bootstrapper.Expect(b => b.EndBootstrapping(null, null)).IgnoreArguments();
            migrator.UseCustomBootstrapping(bootstrapper);

            migrator.MigrateAll(typeof(Migration1).Assembly);

            // assert Migration1 table was *not* created
            DataTable table = GetTable(new Migration1().TableName);
            Assert.IsNull(table);

            // assert Versioning table has necessary entries
            DataTable versioningTable = GetTable(_options.VersioningTableName);
            Assert.AreEqual(Migrations.Count, versioningTable.Rows.Count, "The versioning table is missing entries.");

            bootstrapper.VerifyAllExpectations();
        }

        /// <summary>
        /// Gets the content of the specified table or null if the table does not exist.
        /// </summary>
        protected abstract DataTable GetTable(string tableName);

        protected abstract string ConnectionString { get; }

        protected abstract string ProviderName { get; }

        protected static string GetEnvironmentVariable(string variable)
        {
            string value = Environment.GetEnvironmentVariable(variable);
            if (value == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "No environment variable called '{0}' is defined.", variable));
            }
            return value;
        }

        [SetUp]
        public virtual void Setup()
        {
            _options = new MigrationOptions();
            _options.SupportedProviders.Set(new[] { ProviderName }); // avoid validation errors/warnings from other providers
        }

        [TearDown]
        public virtual void Teardown()
        {
        }
    }
}