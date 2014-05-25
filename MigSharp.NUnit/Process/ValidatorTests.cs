using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;

using MigSharp.Process;
using MigSharp.Providers;

using NUnit.Framework;

using Rhino.Mocks;

namespace MigSharp.NUnit.Process
{
    [TestFixture, Category("smoke")]
    public class ValidatorTests
    {
        private const string ProviderName = "TestProvider";
        private const int MaximumSupportedLength = 10;
        private const string MigrationName = "TestMigration";

        [Test]
        public void VerifyViolatingMaximumDbObjectNameLengthIsReported()
        {
            const string longestName = "Some very long name";

            IRecordedMigration migration = MockRepository.GenerateStub<IRecordedMigration>();
            migration.Expect(m => m.NewObjectNames).Return(new[] { longestName, longestName.Substring(1) });
            migration.Expect(m => m.DataTypes).Return(Enumerable.Empty<UsedDataType>());
            migration.Expect(m => m.Methods).Return(Enumerable.Empty<string>());
            var report = new MigrationReport(MigrationName, "Some other validation error.", migration);

            string errors;
            string warnings;
            Validate(report, out errors, out warnings);

            Assert.AreEqual(
                string.Format(CultureInfo.CurrentCulture, "Error in migration '{0}': Some other validation error.", MigrationName) + Environment.NewLine +
                string.Format(CultureInfo.CurrentCulture, "Migration '{0}' contains object names that are longer than what is supported by '{1}' ('{2}': {3}, supported: {4}).", MigrationName, ProviderName, longestName, longestName.Length, MaximumSupportedLength),
                errors);
            Assert.IsNullOrEmpty(warnings);
        }

        [Test]
        public void VerifyUnsupportedDataTypesAreReported()
        {
            var dataTypes = new List<UsedDataType>
            {
                new UsedDataType(new DataType(DbType.String, 255), false, false), // ok
                new UsedDataType(new DataType(DbType.Currency), false, false), // not supported
                new UsedDataType(new DataType(DbType.Int32), false, false), // ok
                new UsedDataType(new DataType(DbType.Decimal, 20, 10), false, false), // exceeds size and scale
                new UsedDataType(new DataType(DbType.String), false, false), // ok (should not override the primary key status of this DbType)

                new UsedDataType(new DataType(DbType.String), true, false), // as primary key -> *not* ok w/o size
                new UsedDataType(new DataType(DbType.String, 255), true, false), // as primary key -> ok

                new UsedDataType(new DataType(DbType.Decimal, 8, 2), false, true), // as identity -> *not* ok with scale
                new UsedDataType(new DataType(DbType.Decimal, 8), false, true), // as identity -> ok without scale
            };
            IRecordedMigration migration = MockRepository.GenerateStub<IRecordedMigration>();
            migration.Expect(m => m.DataTypes).Return(dataTypes);
            migration.Expect(m => m.Methods).Return(Enumerable.Empty<string>());
            MigrationReport report = new MigrationReport(MigrationName, string.Empty, migration);

            string errors;
            string warnings;
            Validate(report, out errors, out warnings);

            string expected = string.Format(CultureInfo.CurrentCulture, "Migration '{0}' uses the data type '{1}' which is not supported by '{2}'.", MigrationName, DbType.Currency, ProviderName) + Environment.NewLine +
                              string.Format(CultureInfo.CurrentCulture, "Migration '{0}' uses the data type '{1}(20,10)' which exceeds the maximum size of 10 supported by '{2}'.", MigrationName, DbType.Decimal, ProviderName) + Environment.NewLine +
                              string.Format(CultureInfo.CurrentCulture, "Migration '{0}' uses the data type '{1}(20,10)' which exceeds the maximum scale of 5 supported by '{2}'.", MigrationName, DbType.Decimal, ProviderName) + Environment.NewLine +
                              string.Format(CultureInfo.CurrentCulture, "Migration '{0}' uses the data type '{1}' for a primary key which is not supported by '{2}'.", MigrationName, DbType.String, ProviderName) + Environment.NewLine +
                              string.Format(CultureInfo.CurrentCulture, "Migration '{0}' uses the data type '{1}(8,2)' for an identity column which is not supported by '{2}'.", MigrationName, DbType.Decimal, ProviderName);
            Assert.AreEqual(expected, errors);
        }

        [Test]
        public void VerifyWarningsForSupportedDataTypesAreReported()
        {
            MigrationOptions options = GetOptions();
            string warnings;
            string errors = GetWarnings(options, out warnings);

            Assert.IsNullOrEmpty(errors);
            Assert.AreEqual(
                string.Format(CultureInfo.CurrentCulture, "Migration '{0}' uses the data type '{1}' which is not fully supported by '{2}': {3}", MigrationName, DbType.Guid, ProviderName, ProviderStub.WarningMessage) + Environment.NewLine +
                string.Format(CultureInfo.CurrentCulture, "Migration '{0}' uses the data type '{1}' which is not fully supported by '{2}': {3}", MigrationName, DbType.String, ProviderName, ProviderStub.WarningMessageWithoutSize),
                warnings);
        }

        [Test]
        public void VerifyWarningsForSupportedDataTypesAreReportedUnlessSuppressed()
        {
            MigrationOptions options = GetOptions();
            options.SuppressWarning(ProviderName, DbType.Guid, SuppressCondition.WhenSpecifiedWithoutSize);
            string warnings;
            string errors = GetWarnings(options, out warnings);

            Assert.IsNullOrEmpty(errors);
            Assert.AreEqual(
                string.Format(CultureInfo.CurrentCulture, "Migration '{0}' uses the data type '{1}' which is not fully supported by '{2}': {3}", MigrationName, DbType.String, ProviderName, ProviderStub.WarningMessageWithoutSize),
                warnings);
        }

        private static string GetWarnings(MigrationOptions options, out string warnings)
        {
            var dataTypes = new List<UsedDataType>
            {
                new UsedDataType(new DataType(DbType.Guid), false, false),
                new UsedDataType(new DataType(DbType.String), false, false),
            };
            IRecordedMigration migration = MockRepository.GenerateStub<IRecordedMigration>();
            migration.Expect(m => m.DataTypes).Return(dataTypes);
            migration.Expect(m => m.Methods).Return(Enumerable.Empty<string>());
            MigrationReport report = new MigrationReport(MigrationName, string.Empty, migration);

            string errors;
            Validate(options, report, out errors, out warnings);
            return errors;
        }

        [Test]
        public void VerifyWarningsForOdbc()
        {
            var dataTypes = new List<UsedDataType>
            {
                new UsedDataType(new DataType(DbType.Int64), false, false),
            };
            IRecordedMigration migration = MockRepository.GenerateStub<IRecordedMigration>();
            migration.Expect(m => m.DataTypes).Return(dataTypes);
            migration.Expect(m => m.Methods).Return(Enumerable.Empty<string>());
            MigrationReport report = new MigrationReport(MigrationName, string.Empty, migration);

            string errors;
            string warnings;
            Validate(report, out errors, out warnings);

            Assert.IsNullOrEmpty(errors);
            Assert.AreEqual(
                string.Format(CultureInfo.CurrentCulture, "Migration '{0}' uses the data type '{1}' which is not fully supported by '{2}': Int64 is not supported for DbParameters with ODBC; requires calling ToString to directly inline the value in the CommandText.", MigrationName, DbType.Int64, ProviderName),
                warnings);
        }

        [Test]
        public void VerifyWronglyUsedOfSizeAreReported()
        {
            var dataTypes = new List<UsedDataType>
            {
                new UsedDataType(new DataType(DbType.Int32, 777), false, false),
                new UsedDataType(new DataType(DbType.String, null, 777), false, false),
                new UsedDataType(new DataType(DbType.Decimal), false, false),
            };
            IRecordedMigration migration = MockRepository.GenerateStub<IRecordedMigration>();
            migration.Expect(m => m.DataTypes).Return(dataTypes);
            migration.Expect(m => m.Methods).Return(Enumerable.Empty<string>());
            MigrationReport report = new MigrationReport(MigrationName, string.Empty, migration);

            string errors;
            string warnings;
            Validate(report, out errors, out warnings);

            Assert.AreEqual(
                string.Format(CultureInfo.CurrentCulture, "Migration '{0}' uses the data type '{1}(777)' which is not supported by '{2}'.", MigrationName, DbType.Int32, ProviderName) + Environment.NewLine +
                string.Format(CultureInfo.CurrentCulture, "Migration '{0}' uses the data type '{1}(null,777)' which is not supported by '{2}'.", MigrationName, DbType.String, ProviderName) + Environment.NewLine +
                string.Format(CultureInfo.CurrentCulture, "Migration '{0}' uses the data type '{1}' which is not supported by '{2}'.", MigrationName, DbType.Decimal, ProviderName),
                errors);
            Assert.IsNullOrEmpty(warnings);
        }

        [Test]
        public void TestGetUnsupportedMethods() // CLEAN: move to test class of its own
        {
            var provider = new ProviderStub();
            List<UnsupportedMethod> unsupportedMethods = provider.GetUnsupportedMethods().ToList();

            UnsupportedMethod dropTable = unsupportedMethods.Find(m => m.Name == "DropTable");
            Assert.IsNotNull(dropTable, "DropTable should be one of the unsupported methods.");
            Assert.AreEqual(ProviderStub.NotSupportedMessageForDropTable, dropTable.Message);

            Assert.IsNull(unsupportedMethods.Find(m => m.Name == "CreateTable"), "CreateTable should be one of the supported methods.");
        }

        [Test]
        public void VerifyUnsupportedMethodsAreReported()
        {
            IRecordedMigration migration = MockRepository.GenerateStub<IRecordedMigration>();
            migration.Expect(m => m.DataTypes).Return(Enumerable.Empty<UsedDataType>());
            migration.Expect(m => m.Methods).Return(new[] { "CreateTable", "DropTable" });
            MigrationReport report = new MigrationReport(MigrationName, string.Empty, migration);

            string errors;
            string warnings;
            Validate(report, out errors, out warnings);

            Assert.AreEqual(
                string.Format(CultureInfo.CurrentCulture, "Migration '{0}' calls the '{1}' method which is not supported by '{2}': DropTable is not supported because this is just a test.", MigrationName, "DropTable", ProviderName),
                errors);
            Assert.IsNullOrEmpty(warnings);
        }

        private static MigrationOptions GetOptions()
        {
            IProviderMetadata returnedMetadata = MockRepository.GenerateStub<IProviderMetadata>();
            returnedMetadata.Expect(m => m.MaximumDbObjectNameLength).Return(MaximumSupportedLength);
            returnedMetadata.Expect(m => m.Name).Return(ProviderName);
            returnedMetadata.Expect(m => m.InvariantName).Return("System.Data.Odbc"); // for the Odbc specific tests

            IProvider provider = new ProviderStub();

            IProviderFactory providerFactory = MockRepository.GenerateStub<IProviderFactory>();
            IProviderMetadata passedMetadata;
            providerFactory.Expect(f => f.GetProvider(ProviderName, out passedMetadata)).OutRef(returnedMetadata).Return(provider);

            var supportedProviders = new SupportedProviders(providerFactory);
            supportedProviders.Add(ProviderName);

            var options = new MigrationOptions();
            options.SupportedProviders = supportedProviders;
            return options;
        }

        private static void Validate(MigrationOptions options, IMigrationReport report, out string errors, out string warnings)
        {
            var validator = new Validator(options);
            IMigrationReporter reporter = MockRepository.GenerateStub<IMigrationReporter>();
            reporter.Expect(r => r.Report(null)).IgnoreArguments().Return(report);
            IMigrationReporter[] reporters = new[] { reporter };
            validator.Validate(reporters, out errors, out warnings);
        }

        private static void Validate(IMigrationReport report, out string errors, out string warnings)
        {
            Validate(GetOptions(), report, out errors, out warnings);
        }

        [Supports(DbType.Int32)]
        [Supports(DbType.Int64)]
        [Supports(DbType.Decimal, MaximumSize = 10, MaximumScale = 5)]
        [Supports(DbType.Decimal, MaximumSize = 10, CanBeUsedAsIdentity = true)]
        [Supports(DbType.Guid, Warning = WarningMessage)]
        [Supports(DbType.String, MaximumSize = 2000, CanBeUsedAsPrimaryKey = true)]
        [Supports(DbType.String, Warning = WarningMessageWithoutSize)]
        private class ProviderStub : IProvider
        {
            public const string WarningMessage = "This is a test warning message.";
            public const string WarningMessageWithoutSize = "Some warning that applies when size is omitted.";
            public const string NotSupportedMessageForDropTable = "DropTable is not supported because this is just a test.";

            #region Implementation of IProvider

            string IProvider.ExistsTable(string databaseName, string tableName)
            {
                return string.Empty;
            }

            public string ConvertToSql(object value, DbType targetDbType)
            {
                throw new NotSupportedException();
            }

            IEnumerable<string> IProvider.CreateTable(string tableName, IEnumerable<CreatedColumn> columns, string primaryKeyConstraintName)
            {
                yield break;
            }

            IEnumerable<string> IProvider.DropTable(string tableName)
            {
                throw new NotSupportedException(NotSupportedMessageForDropTable);
            }

            IEnumerable<string> IProvider.AddColumn(string tableName, Column column)
            {
                throw new NotSupportedException();
            }

            IEnumerable<string> IProvider.RenameTable(string oldName, string newName)
            {
                throw new NotSupportedException();
            }

            IEnumerable<string> IProvider.RenameColumn(string tableName, string oldName, string newName)
            {
                throw new NotSupportedException();
            }

            IEnumerable<string> IProvider.DropColumn(string tableName, string columnName)
            {
                throw new NotSupportedException();
            }

            IEnumerable<string> IProvider.AlterColumn(string tableName, Column column)
            {
                throw new NotSupportedException();
            }

            public IEnumerable<string> AddIndex(string tableName, IEnumerable<string> columnNames, string indexName)
            {
                throw new NotSupportedException();
            }

            public IEnumerable<string> DropIndex(string tableName, string indexName)
            {
                throw new NotSupportedException();
            }

            IEnumerable<string> IProvider.AddForeignKey(string tableName, string referencedTableName, IEnumerable<ColumnReference> columnNames, string constraintName)
            {
                throw new NotSupportedException();
            }

            IEnumerable<string> IProvider.DropForeignKey(string tableName, string constraintName)
            {
                throw new NotSupportedException();
            }

            IEnumerable<string> IProvider.AddPrimaryKey(string tableName, IEnumerable<string> columnNames, string constraintName)
            {
                throw new NotSupportedException();
            }

            public IEnumerable<string> RenamePrimaryKey(string tableName, string oldName, string newName)
            {
                throw new NotSupportedException();
            }

            IEnumerable<string> IProvider.DropPrimaryKey(string tableName, string constraintName)
            {
                throw new NotSupportedException();
            }

            IEnumerable<string> IProvider.AddUniqueConstraint(string tableName, IEnumerable<string> columnNames, string constraintName)
            {
                throw new NotSupportedException();
            }

            IEnumerable<string> IProvider.DropUniqueConstraint(string tableName, string constraintName)
            {
                throw new NotSupportedException();
            }

            public IEnumerable<string> DropDefault(string tableName, Column column)
            {
                throw new NotSupportedException();
            }

            #endregion
        }
    }
}