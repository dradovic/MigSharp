using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

using MigSharp.NUnit.Migrate;
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

        private static readonly IMigrationTimestampProvider TimestampProvider = new DefaultMigrationTimestampProvider();

        private MigrationOptions _options;

        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static IntegrationTestsBase()
        {
            const string pattern = @"Migration(\d+)";
            Migrations.AddRange(typeof(IntegrationTestsBase).Assembly.GetTypes()
                .Where(t => Regex.IsMatch(t.Name, pattern) && t.GetInterface("IMigration") != null)
                .OrderBy(t => long.Parse(Regex.Match(t.Name, pattern).Groups[1].Value, CultureInfo.InvariantCulture))
                .Select(t => (IMigration)Activator.CreateInstance(t)));
            Timestamps = new List<long>(Migrations.Select(m => TimestampProvider.GetTimestamp(m.GetType())));
            MigrationOptions.SetGeneralTraceLevel(SourceLevels.All);
            MigrationOptions.SetPerformanceTraceLevel(SourceLevels.All);
            MigrationOptions.SetSqlTraceLevel(SourceLevels.All);
        }

        [Test]
        public void TestOnlyFetchingMigrationsDoesNotCreateVersioningTable()
        {
            _options.VersioningTableName = "My Versioning Table"; // test overriding the default versioning table name
            Migrator migrator = CreateMigrator();
            IMigrationBatch batch = migrator.FetchMigrations(typeof(Migration1).Assembly);
            Assert.AreEqual(Timestamps.Count, batch.ScheduledMigrations.Count);

            DataTable versioningTable = GetTable(_options.VersioningTableName);
            Assert.IsNull(versioningTable, "Migrator.IsUpToDate should not have any side-effects. In particualar, it should *not* create a versioning table. This allows for being able to check the up-to-dateness of a db without having the privilege to create tables.");
        }

        private Migrator CreateMigrator()
        {
            var migrator = new Migrator(ConnectionString, ProviderName, _options);
            if (CustomConnection != null)
            {
                migrator.UseCustomConnection(CustomConnection);
            }
            return migrator;
        }

        [Test]
        public void TestMigration1()
        {
            _options.VersioningTableName = "My Versioning Table"; // test overriding the default versioning table name
            Migrator migrator = CreateMigrator();

            // verify if the migrations batch is populated correctly
            IMigrationBatch batch = migrator.FetchMigrationsTo(typeof(Migration1).Assembly, Timestamps[0]);
            Assert.AreEqual(1, batch.ScheduledMigrations.Count);
            Assert.AreEqual(Timestamps[0], batch.ScheduledMigrations[0].Timestamp);
            Assert.AreEqual(MigrationExportAttribute.DefaultModuleName, batch.ScheduledMigrations[0].ModuleName);
            Assert.IsNull(batch.ScheduledMigrations[0].Tag);
            Assert.AreEqual(MigrationDirection.Up, batch.ScheduledMigrations[0].Direction);

            // use MigrateTo to execute the actual migrations to test that method, too
            migrator.MigrateTo(typeof(Migration1).Assembly, Timestamps[0]);

            CheckResultsOfMigration1();
        }

        private void CheckResultsOfMigration1()
        {
            // assert Versioning table was created
            DataTable versioningTable = GetTable(_options.VersioningTableName);
            Assert.IsNotNull(versioningTable, string.Format(CultureInfo.CurrentCulture, "The '{0}' table was not created.", _options.VersioningTableName));
            Assert.AreEqual(3, versioningTable.Columns.Count);
            Assert.AreEqual(BootstrapMigration.TimestampColumnName, versioningTable.Columns[0].ColumnName);
            Assert.AreEqual(BootstrapMigration.ModuleColumnName, versioningTable.Columns[1].ColumnName);
            Assert.AreEqual(BootstrapMigration.TagColumnName, versioningTable.Columns[2].ColumnName);

            // assert Customer table was created
            Migration1 migration1 = new Migration1();
            DataTable customerTable = GetTable(migration1.Tables[0].Name);
            Assert.IsNotNull(customerTable, string.Format(CultureInfo.CurrentCulture, "The '{0}' table was not created.", migration1.Tables[0].Name));
            Assert.AreEqual(1, customerTable.Columns.Count);
            Assert.AreEqual(migration1.Tables[0].Columns[0], customerTable.Columns[0].ColumnName);

            // assert Versioning table has necessary entries
            Assert.AreEqual(1, versioningTable.Rows.Count, "The versioning table is missing entries.");
            Assert.AreEqual(Timestamps[0], versioningTable.Rows[0][0], "The timestamp of Migration1 is wrong.");
            Assert.AreEqual(MigrationExportAttribute.DefaultModuleName, versioningTable.Rows[0][1], "The module of Migration1 is wrong.");
            Assert.AreEqual(DBNull.Value, versioningTable.Rows[0][2], "The tag of Migration1 is wrong.");
        }

        [Test]
        public void TestScriptingAndExecutingMigration1()
        {
            DirectoryInfo targetDirectory = PrepareScriptingDirectory();
            _options.VersioningTableName = "My Versioning Table"; // test overriding the default versioning table name
            _options.ExecuteAndScriptSqlTo(targetDirectory);
            Migrator migrator = CreateMigrator();
            migrator.MigrateTo(typeof(Migration1).Assembly, Timestamps[0]);

            CheckResultsOfMigration1();

            // assert that the script file was generated
            FileInfo[] scriptFiles = targetDirectory.GetFiles(string.Format(CultureInfo.InvariantCulture, "Migration." + MigrationExportAttribute.DefaultModuleName + ".1.sql"));
            Assert.AreEqual(1, scriptFiles.Length);

            // delete script files
            targetDirectory.Delete(true);
        }

        private static DirectoryInfo PrepareScriptingDirectory()
        {
            string tempPath = Path.GetTempPath();
            string targetPath = Path.Combine(tempPath, "MigSharp");
            var targetDirectory = new DirectoryInfo(targetPath);
            if (!targetDirectory.Exists)
            {
                targetDirectory.Create();
                targetDirectory = new DirectoryInfo(targetPath); // it seems that the Exists flag is not updated by the call to Create
            }
            return targetDirectory;
        }

        [Test]
        public virtual void TestMigration1UsingConsoleApp()
        {
            int exitCode = MigrateProcess.Execute(ConnectionString, ProviderName, typeof(Migration1).Assembly, Timestamps[0]);
            Assert.AreEqual(0, exitCode, "Migrate.exe failed.");
            CheckResultsOfMigration1();
        }

        [Test]
        public void TestScriptingAllMigrations()
        {
            DirectoryInfo targetDirectory = PrepareScriptingDirectory();
            _options.VersioningTableName = "My Versioning Table"; // test overriding the default versioning table name
            _options.OnlyScriptSqlTo(targetDirectory);
            Migrator migrator = CreateMigrator();
            migrator.MigrateAll(typeof(Migration1).Assembly);

            // assert that all script files were generated
            List<FileInfo> scriptFiles = targetDirectory.GetFiles(string.Format(CultureInfo.InvariantCulture, "Migration.*.*.sql"))
                .OrderBy(f => int.Parse(Regex.Match(f.Name, @"Migration\..*\.(\d+)\.sql").Groups[1].Value, CultureInfo.InvariantCulture))
                .ToList();
            Assert.AreEqual(Migrations.Count, scriptFiles.Count);
            Assert.AreEqual("Migration." + MigrationExportAttribute.DefaultModuleName + ".1.sql", scriptFiles[0].Name);
            Assert.AreEqual("Migration." + Migration2.Module + ".2.sql", scriptFiles[1].Name);

            // assert Versioning table was *not* created as we are scripting only
            DataTable versioningTable = GetTable(_options.VersioningTableName);
            Assert.IsNull(versioningTable, string.Format(CultureInfo.CurrentCulture, "The '{0}' table was created altough ScriptingMode was ScriptOnly.", _options.VersioningTableName));

            // assert Customer table was *not* created as we are scripting only
            var migration1 = new Migration1();
            DataTable customerTable = GetTable(migration1.Tables[0].Name);
            Assert.IsNull(customerTable, string.Format(CultureInfo.CurrentCulture, "The '{0}' table was created altough ScriptingMode was ScriptOnly.", migration1.Tables[0].Name));

            // execute generated script files against database and recheck results
            IProviderMetadata providerMetadata;
            _options.SupportedProviders.GetProvider(ProviderName, out providerMetadata);
            var info = new ConnectionInfo(ConnectionString, providerMetadata.InvariantName, providerMetadata.SupportsTransactions, providerMetadata.EnableAnsiQuotesCommand);
            using (IDbConnection connection = migrator.ConnectionFactory.OpenConnection(info))
            {
                foreach (FileInfo scriptFile in scriptFiles)
                {
                    Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Reading script '{0}':", scriptFile.FullName));
                    string[] scriptLines = File.ReadAllLines(scriptFile.FullName);
                    foreach (string line in scriptLines)
                    {
                        Trace.WriteLine(line);
                    }

                    // group all lines between empty lines into one command (some database platforms require DDL operations to
                    // be executed in separated commands)
                    Trace.WriteLine(Environment.NewLine + string.Format(CultureInfo.CurrentCulture, "Executing script '{0}':", scriptFile.FullName));
                    string commandText = string.Empty;
                    foreach (string line in scriptLines)
                    {
                        if (line.Trim().Length != 0)
                        {
                            commandText += line;
                        }
                        else
                        {
                            ExecuteCommand(commandText, connection);
                            commandText = string.Empty;
                        }
                    }
                    Assert.IsEmpty(commandText, "The script should end with an empty line.");
                }
            }
            VerifyResultsOfAllMigrations();

            // delete script files
            targetDirectory.Delete(true);
        }

        private static void ExecuteCommand(string batch, IDbConnection connection)
        {
            IDbCommand command = connection.CreateCommand();
            command.CommandText = batch;
            Trace.WriteLine(command.CommandText);
            command.ExecuteNonQuery();
        }

        [Test]
        public void TestMigration1SucceededByAllOtherMigrations()
        {
            // execute Migration1
            Migrator migrator = CreateMigrator();
            Assembly assemblyContainingMigrations = typeof(Migration1).Assembly;
            migrator.MigrateTo(assemblyContainingMigrations, Timestamps[0]);

            // execute all other migrations
            migrator = CreateMigrator();
            migrator.MigrateAll(assemblyContainingMigrations);

            // make sure there are no more migrations to run
            IMigrationBatch batch = migrator.FetchMigrations(assemblyContainingMigrations);
            Assert.AreEqual(0, batch.ScheduledMigrations.Count);

            VerifyResultsOfAllMigrations();
        }

        private void VerifyResultsOfAllMigrations()
        {
            // assert all tables have been created with the expected content
            foreach (IIntegrationTestMigration migration in Migrations.OfType<IIntegrationTestMigration>())
            {
                foreach (ExpectedTable expectedTable in migration.Tables)
                {
                    DataTable table = GetTable(expectedTable.Name);

                    Assert.IsNotNull(table, string.Format(CultureInfo.CurrentCulture, "The table '{0}' was not created.", expectedTable.Name));
                    Assert.AreEqual(expectedTable.Columns.Count, table.Columns.Count, "The actual number of columns is wrong.");
                    Assert.AreEqual(expectedTable.Count, table.Rows.Count, "The actual number of rows is wrong.");
                    for (int column = 0; column < expectedTable.Columns.Count; column++)
                    {
                        // check column name
                        Assert.AreEqual(expectedTable.Columns[column], table.Columns[column].ColumnName,
                            string.Format(CultureInfo.CurrentCulture, "A column name of table '{0}' is wrong.", expectedTable.Name));

                        // check content
                        for (int row = 0; row < expectedTable.Count; row++)
                        {
                            object expectedValue = expectedTable.Value(row, column);
                            object actualValue = table.Rows[row][column];
                            Func<object, bool> evalValue = expectedValue as Func<object, bool>;
                            if (evalValue != null)
                            {
                                Assert.IsTrue(evalValue(actualValue), string.Format(CultureInfo.CurrentCulture, "In '{0}', the actual value of cell {1}/{2} of table '{3}' is wrong (the custom handler returned false).",
                                    migration.GetType().Name,
                                    row,
                                    column,
                                    expectedTable.Name));
                            }
                            else
                            {
                                Assert.AreEqual(expectedValue, actualValue, string.Format(CultureInfo.CurrentCulture, "In '{0}', the actual value of cell {1}/{2} of table '{3}' is wrong.",
                                    migration.GetType().Name,
                                    row,
                                    column,
                                    expectedTable.Name));
                            }
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
            Migrator migrator = CreateMigrator();
            Assembly assemblyContainingMigrations = typeof(Migration1).Assembly;
            migrator.MigrateTo(assemblyContainingMigrations, Timestamps[1]);

            migrator = CreateMigrator();

            // verify if the migrations batch is populated correctly
            IMigrationBatch batch = migrator.FetchMigrationsTo(assemblyContainingMigrations, Timestamps[0]);
            Assert.AreEqual(1, batch.ScheduledMigrations.Count, "Only the reversal of Migration2 should be scheduled.");
            Assert.AreEqual(Timestamps[1], batch.ScheduledMigrations[0].Timestamp);
            Assert.AreEqual(Migration2.Module, batch.ScheduledMigrations[0].ModuleName);
            Assert.AreEqual(Migration2.Tag, batch.ScheduledMigrations[0].Tag);
            Assert.AreEqual(MigrationDirection.Down, batch.ScheduledMigrations[0].Direction);

            // use MigrateTo to execute the actual migrations to test that method, too
            migrator.MigrateTo(assemblyContainingMigrations, Timestamps[0]);

            // assert order table was dropped
            DataTable orderTable = GetTable(new Migration2().Tables[0].Name);
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
            Migrator migrator = CreateMigrator();

            IBootstrapper bootstrapper = MockRepository.GenerateStrictMock<IBootstrapper>();
            bootstrapper.Expect(b => b.BeginBootstrapping(null, null)).IgnoreArguments();
            for (int i = 0; i < Migrations.Count; i++) // assume that all migrations were already performed
            {
                long timestamp = Timestamps[i];
                bootstrapper.Expect(b => b.IsContained(Arg<IMigrationMetadata>.Matches(m => m.Timestamp == timestamp))).Return(true);
            }
            bootstrapper.Expect(b => b.EndBootstrapping(null, null)).IgnoreArguments();
            migrator.UseCustomBootstrapping(bootstrapper);

            IMigrationBatch batch = migrator.FetchMigrations(typeof(Migration1).Assembly);
            CollectionAssert.IsEmpty(batch.ScheduledMigrations, "All migrations should be viewed as already run.");
            CollectionAssert.IsEmpty(batch.UnidentifiedMigrations, "There should be no unidentified migrations.");
            batch.Execute(); // should have no effect as no migrations are scheduled

            migrator.MigrateAll(typeof(Migration1).Assembly); // should have no effect as no migrations are scheduled

            // assert Migration1 table was *not* created
            DataTable table = GetTable(new Migration1().Tables[0].Name);
            Assert.IsNull(table);

            // assert Versioning table has necessary entries
            DataTable versioningTable = GetTable(_options.VersioningTableName);
            Assert.AreEqual(Migrations.Count, versioningTable.Rows.Count, "The versioning table is missing entries.");

            bootstrapper.VerifyAllExpectations();
        }

        [Test]
        public void TestUnidentifiedMigrations()
        {
            if (ProviderName == ProviderNames.Teradata || ProviderName == ProviderNames.TeradataOdbc ||
                ProviderName == ProviderNames.Oracle || ProviderName == ProviderNames.OracleOdbc) return; // for some reason, the ODBC data adapter updating does not work

            // migrate to 1 in order to create a versioning table
            Migrator migrator = CreateMigrator();
            IMigrationBatch batch = migrator.FetchMigrationsTo(typeof(Migration1).Assembly, Timestamps[0]);
            Assert.AreEqual(0, batch.UnidentifiedMigrations.Count);
            batch.Execute();

            // insert unidentified migrations into the versioning table
            DataTable versioningTable = GetTable(_options.VersioningTableName);
            const long timestamp = 123456L;
            const string moduleName = "Test";
            const string tag = "This migration is not known to the application.";
            versioningTable.Rows.Add(timestamp, moduleName, tag);
            SaveTable(versioningTable);

            // verify if the UnidentifiedMigrations is populated correctly
            batch = migrator.FetchMigrations(typeof(Migration1).Assembly);
            Assert.AreEqual(1, batch.UnidentifiedMigrations.Count);
            Assert.AreEqual(timestamp, batch.UnidentifiedMigrations[0].Timestamp);
            Assert.AreEqual(moduleName, batch.UnidentifiedMigrations[0].ModuleName);
            Assert.AreEqual(tag, batch.UnidentifiedMigrations[0].Tag);
        }

        /// <summary>
        /// Gets the content of the specified table or null if the table does not exist.
        /// </summary>
        private DataTable GetTable(string tableName)
        {
            var table = new DataTable(tableName) { Locale = CultureInfo.InvariantCulture };
            try
            {
                DbCommandBuilder builder;
                var adapter = GetDataAdapter(tableName, out builder);
                {
                    adapter.Fill(table);
                }
            }
            catch (DbException)
            {
                table = null;
            }
            catch (Exception x)
            {
                // unfortunately the SqlCeException (used by SQL Server CE 3.5) does not derive from DbException
                if (x.GetType().Name == "SqlCeException")
                {
                    table = null;
                }
                else
                {
                    throw;
                }
            }
            return table;
        }

        private void SaveTable(DataTable table)
        {
            DbDataAdapter adapter;
            DbCommandBuilder builder;
            using (adapter = GetDataAdapter(table.TableName, out builder))
            {
                adapter.InsertCommand = builder.GetInsertCommand();
                adapter.Update(table);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#")]
        protected abstract DbDataAdapter GetDataAdapter(string tableName, out DbCommandBuilder builder);

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

            // initialize IntegrationTestContext
            IProviderMetadata providerMetadata;
            IProvider provider = _options.SupportedProviders.GetProvider(ProviderName, out providerMetadata);
            IntegrationTestContext.Initialize(_options, provider.GetSupportsAttributes());
        }

        [TearDown]
        public virtual void Teardown()
        {
        }

        /// <summary>
        /// Override if you want to use a custom connection.
        /// </summary>
        /// <returns></returns>
        protected virtual IDbConnection CustomConnection
        {
            get { return null; }
        }
    }
}