using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using FakeItEasy;
using MigSharp.Core;
using MigSharp.Process;
using MigSharp.Providers;

using NUnit.Framework;

namespace MigSharp.NUnit.Process
{
    [TestFixture, Category("smoke")]
    public class ValidatorTests
    {
        private static readonly DbPlatform Platform = DbPlatform.SqlServer2008;
        private const int MaximumSupportedLength = 10;
        private const string MigrationName = "TestMigration";

        [Test]
        public void VerifyViolatingMaximumDbObjectNameLengthIsReported()
        {
            const string longestName = "Some very long name";

            IRecordedMigration migration = A.Fake<IRecordedMigration>();
            A.CallTo(() => migration.NewObjectNames).Returns(new[] { longestName, longestName.Substring(1) });
            //A.CallTo(() => migration.DataTypes).Returns(Enumerable.Empty<UsedDataType>());
            //migration.Expect(m => m.Methods).Return(Enumerable.Empty<string>());
            var report = new MigrationReport(MigrationName, "Some other validation error.", migration);

            string errors;
            string warnings;
            Validate(report, out errors, out warnings);

            Assert.AreEqual(
                string.Format(CultureInfo.CurrentCulture, "Error in migration '{0}': Some other validation error.", MigrationName) + Environment.NewLine +
                string.Format(CultureInfo.CurrentCulture, "Migration '{0}' contains object names that are longer than what is supported by '{1}' ('{2}': {3}, supported: {4}).", MigrationName, Platform, longestName, longestName.Length, MaximumSupportedLength),
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
            IRecordedMigration migration = A.Fake<IRecordedMigration>();
            A.CallTo(() => migration.DataTypes).Returns(dataTypes);
            //migration.Expect(m => m.Methods).Return(Enumerable.Empty<string>());
            MigrationReport report = new MigrationReport(MigrationName, string.Empty, migration);

            string errors;
            string warnings;
            Validate(report, out errors, out warnings);

            string expected = string.Format(CultureInfo.CurrentCulture, "Migration '{0}' uses the data type '{1}' which is not supported by '{2}'.", MigrationName, DbType.Currency, Platform) + Environment.NewLine +
                              string.Format(CultureInfo.CurrentCulture, "Migration '{0}' uses the data type '{1}(20,10)' which exceeds the maximum size of 10 supported by '{2}'.", MigrationName, DbType.Decimal, Platform) + Environment.NewLine +
                              string.Format(CultureInfo.CurrentCulture, "Migration '{0}' uses the data type '{1}(20,10)' which exceeds the maximum scale of 5 supported by '{2}'.", MigrationName, DbType.Decimal, Platform) + Environment.NewLine +
                              string.Format(CultureInfo.CurrentCulture, "Migration '{0}' uses the data type '{1}' for a primary key which is not supported by '{2}'.", MigrationName, DbType.String, Platform) + Environment.NewLine +
                              string.Format(CultureInfo.CurrentCulture, "Migration '{0}' uses the data type '{1}(8,2)' for an identity column which is not supported by '{2}'.", MigrationName, DbType.Decimal, Platform);
            Assert.AreEqual(expected, errors);
        }

        [Test]
        public void VerifyWarningsForSupportedDataTypesAreReported()
        {
            IEnumerable<ProviderInfo> providerInfos;
            MigrationOptions options = GetOptions(out providerInfos);
            string warnings;
            string errors = GetWarnings(providerInfos, options, out warnings);

            Assert.IsNullOrEmpty(errors);
            Assert.AreEqual(
                string.Format(CultureInfo.CurrentCulture, "Migration '{0}' uses the data type '{1}' which is not fully supported by '{2}': {3}", MigrationName, DbType.Guid, Platform, ProviderStub.WarningMessage) + Environment.NewLine +
                string.Format(CultureInfo.CurrentCulture, "Migration '{0}' uses the data type '{1}' which is not fully supported by '{2}': {3}", MigrationName, DbType.String, Platform, ProviderStub.WarningMessageWithoutSize),
                warnings);
        }

        [Test]
        public void VerifyWarningsForSupportedDataTypesAreReportedUnlessSuppressed()
        {
            IEnumerable<ProviderInfo> providerInfos;
            MigrationOptions options = GetOptions(out providerInfos);
            options.SuppressWarning(Platform, DbType.Guid, SuppressCondition.WhenSpecifiedWithoutSize);
            string warnings;
            string errors = GetWarnings(providerInfos, options, out warnings);

            Assert.IsNullOrEmpty(errors);
            Assert.AreEqual(
                string.Format(CultureInfo.CurrentCulture, "Migration '{0}' uses the data type '{1}' which is not fully supported by '{2}': {3}", MigrationName, DbType.String, Platform, ProviderStub.WarningMessageWithoutSize),
                warnings);
        }

        private static string GetWarnings(IEnumerable<ProviderInfo> providerInfos, MigrationOptions options, out string warnings)
        {
            var dataTypes = new List<UsedDataType>
            {
                new UsedDataType(new DataType(DbType.Guid), false, false),
                new UsedDataType(new DataType(DbType.String), false, false),
            };
            IRecordedMigration migration = A.Fake<IRecordedMigration>();
            A.CallTo(() => migration.DataTypes).Returns(dataTypes);
            //migration.Expect(m => m.Methods).Return(Enumerable.Empty<string>());
            MigrationReport report = new MigrationReport(MigrationName, string.Empty, migration);

            string errors;
            Validate(providerInfos, options, report, out errors, out warnings);
            return errors;
        }

        [Test]
        public void VerifyWarningsForOdbc()
        {
            var dataTypes = new List<UsedDataType>
            {
                new UsedDataType(new DataType(DbType.Int64), false, false),
            };
            IRecordedMigration migration = A.Fake<IRecordedMigration>();
            A.CallTo(() => migration.DataTypes).Returns(dataTypes);
            //migration.Expect(m => m.Methods).Return(Enumerable.Empty<string>());
            MigrationReport report = new MigrationReport(MigrationName, string.Empty, migration);

            string errors;
            string warnings;
            Validate(report, out errors, out warnings);

            Assert.IsNullOrEmpty(errors);
            Assert.AreEqual(
                string.Format(CultureInfo.CurrentCulture, "Migration '{0}' uses the data type '{1}' which is not fully supported by '{2}': Int64 is not supported for DbParameters with ODBC; requires calling ToString to directly inline the value in the CommandText.", MigrationName, DbType.Int64, Platform),
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
            IRecordedMigration migration = A.Fake<IRecordedMigration>();
            A.CallTo(() => migration.DataTypes).Returns(dataTypes);
            //migration.Expect(m => m.Methods).Return(Enumerable.Empty<string>());
            MigrationReport report = new MigrationReport(MigrationName, string.Empty, migration);

            string errors;
            string warnings;
            Validate(report, out errors, out warnings);

            Assert.AreEqual(
                string.Format(CultureInfo.CurrentCulture, "Migration '{0}' uses the data type '{1}(777)' which is not supported by '{2}'.", MigrationName, DbType.Int32, Platform) + Environment.NewLine +
                string.Format(CultureInfo.CurrentCulture, "Migration '{0}' uses the data type '{1}(null,777)' which is not supported by '{2}'.", MigrationName, DbType.String, Platform) + Environment.NewLine +
                string.Format(CultureInfo.CurrentCulture, "Migration '{0}' uses the data type '{1}' which is not supported by '{2}'.", MigrationName, DbType.Decimal, Platform),
                errors);
            Assert.IsNullOrEmpty(warnings);
        }

        [Test]
        public void TestGetUnsupportedMethods() // CLEAN: move to test class of its own
        {
            var provider = new ProviderStub();
            List<UnsupportedMethod> unsupportedMethods = provider.GetUnsupportedMethods().ToList();

            UnsupportedMethod addColumn = unsupportedMethods.Find(m => m.Name == "AddColumn");
            Assert.IsNotNull(addColumn, "AddColumn should be one of the unsupported methods.");
            Assert.AreEqual(ProviderStub.NotSupportedMessageForAddColumn, addColumn.Message);

            UnsupportedMethod dropTable = unsupportedMethods.Find(m => m.Name == "DropTableIfExists");
            Assert.IsNotNull(dropTable, "DropTableIfExists should be one of the unsupported methods.");

            Assert.IsNull(unsupportedMethods.Find(m => m.Name == "CreateTable"), "CreateTable should be one of the supported methods.");
        }

        [Test]
        public void VerifyUnsupportedMethodsAreReported()
        {
            IRecordedMigration migration = A.Fake<IRecordedMigration>();
            //migration.Expect(m => m.DataTypes).Return(Enumerable.Empty<UsedDataType>());
            A.CallTo(() => migration.Methods).Returns(new[] { "CreateTable", "AddColumn" });
            MigrationReport report = new MigrationReport(MigrationName, string.Empty, migration);

            string errors;
            string warnings;
            Validate(report, out errors, out warnings);

            Assert.AreEqual(
                string.Format(CultureInfo.CurrentCulture, "Migration '{0}' calls the '{1}' method which is not supported by '{2}': AddColumn is not supported because this is just a test.", MigrationName, "AddColumn", Platform),
                errors);
            Assert.IsNullOrEmpty(warnings);
        }

        private static MigrationOptions GetOptions(out IEnumerable<ProviderInfo> providerInfos)
        {
            IProviderMetadata metadata = A.Fake<IProviderMetadata>();
            A.CallTo(() => metadata.MaximumDbObjectNameLength).Returns(MaximumSupportedLength);
            A.CallTo(() => metadata.MajorVersion).Returns(Platform.MajorVersion);
            A.CallTo(() => metadata.InvariantName).Returns("System.Data.Odbc"); // for the Odbc specific tests

            IProvider provider = new ProviderStub();

            IProviderRegistry providerRegistry = A.Fake<IProviderRegistry>();
            A.CallTo(() => providerRegistry.GetProvider(metadata)).Returns(provider);
            providerInfos = new[] { new ProviderInfo(provider, metadata) };

            var options = new MigrationOptions();
            options.SupportedPlatforms.AddOrReplaceMinimumRequirement(Platform);
            return options;
        }

        private static void Validate(IEnumerable<ProviderInfo> providerInfos, MigrationOptions options, IMigrationReport report, out string errors, out string warnings)
        {
            var validator = new Validator(providerInfos, options);
            IMigrationReporter reporter = A.Fake<IMigrationReporter>();
            A.CallTo(() => reporter.Report(A<IMigrationContext>._)).Returns(report);
            IMigrationReporter[] reporters = new[] { reporter };
            validator.Validate(reporters, out errors, out warnings);
        }

        private static void Validate(IMigrationReport report, out string errors, out string warnings)
        {
            IEnumerable<ProviderInfo> providerInfos;
            MigrationOptions options = GetOptions(out providerInfos);
            Validate(providerInfos, options, report, out errors, out warnings);
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
            public const string NotSupportedMessageForAddColumn = "AddColumn is not supported because this is just a test.";

            #region Implementation of IProvider

            string IProvider.ExistsTable(string databaseName, TableName tableName)
            {
                return string.Empty;
            }

            public string ConvertToSql(object value, DbType targetDbType)
            {
                throw new NotSupportedException();
            }

            IEnumerable<string> IProvider.CreateTable(TableName tableName, IEnumerable<CreatedColumn> columns, string primaryKeyConstraintName)
            {
                yield break;
            }

            IEnumerable<string> IProvider.DropTable(TableName tableName, bool checkIfExists)
            {
                if (checkIfExists)
                {
                    throw new NotSupportedException();
                }
                return Enumerable.Empty<string>();
            }

            IEnumerable<string> IProvider.AddColumn(TableName tableName, Column column)
            {
                throw new NotSupportedException(NotSupportedMessageForAddColumn);
            }

            IEnumerable<string> IProvider.RenameTable(TableName oldName, string newName)
            {
                throw new NotSupportedException();
            }

            IEnumerable<string> IProvider.RenameColumn(TableName tableName, string oldName, string newName)
            {
                throw new NotSupportedException();
            }

            IEnumerable<string> IProvider.DropColumn(TableName tableName, string columnName)
            {
                throw new NotSupportedException();
            }

            IEnumerable<string> IProvider.AlterColumn(TableName tableName, Column column)
            {
                throw new NotSupportedException();
            }

            public IEnumerable<string> AddIndex(TableName tableName, IEnumerable<string> columnNames, string indexName)
            {
                throw new NotSupportedException();
            }

            public IEnumerable<string> DropIndex(TableName tableName, string indexName)
            {
                throw new NotSupportedException();
            }

            IEnumerable<string> IProvider.AddForeignKey(TableName tableName, TableName referencedTableName, IEnumerable<ColumnReference> columnNames, string constraintName, bool cascadeOnDelete)
            {
                throw new NotSupportedException();
            }

            IEnumerable<string> IProvider.DropForeignKey(TableName tableName, string constraintName)
            {
                throw new NotSupportedException();
            }

            IEnumerable<string> IProvider.AddPrimaryKey(TableName tableName, IEnumerable<string> columnNames, string constraintName)
            {
                throw new NotSupportedException();
            }

            public IEnumerable<string> RenamePrimaryKey(TableName tableName, string oldName, string newName)
            {
                throw new NotSupportedException();
            }

            IEnumerable<string> IProvider.DropPrimaryKey(TableName tableName, string constraintName)
            {
                throw new NotSupportedException();
            }

            IEnumerable<string> IProvider.AddUniqueConstraint(TableName tableName, IEnumerable<string> columnNames, string constraintName)
            {
                throw new NotSupportedException();
            }

            IEnumerable<string> IProvider.DropUniqueConstraint(TableName tableName, string constraintName)
            {
                throw new NotSupportedException();
            }

            public IEnumerable<string> DropDefault(TableName tableName, Column column)
            {
                throw new NotSupportedException();
            }

            public IEnumerable<string> CreateSchema(string schemaName)
            {
                throw new NotSupportedException();
            }

            public IEnumerable<string> DropSchema(string schemaName)
            {
                throw new NotSupportedException();
            }

            #endregion
        }
    }
}