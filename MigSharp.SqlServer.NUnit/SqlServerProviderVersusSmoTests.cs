using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using MigSharp.Core;
using MigSharp.Core.Entities;
using MigSharp.NUnit;
using MigSharp.Process;
using MigSharp.Providers;

using NUnit.Framework;

using Rhino.Mocks;

namespace MigSharp.SqlServer.NUnit
{
    [TestFixture, Category("SqlServer2008")]
    public class SqlServerProviderVersusSmoTests
    {
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "expectedDataTypes", Justification = "This parameter is provided by the test source but used in a different test.")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "expectedPrimaryKeyDataTypes", Justification = "This parameter is provided by the test source but used in a different test.")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "expectedLongestName", Justification = "This parameter is provided by the test source but used in a different test.")]
        [Test, TestCaseSource(typeof(TestCaseGenerator), "CreateDatabaseCases")]
        public void TestDatabaseCases(IDatabase database, IEnumerable<DataType> expectedDataTypes, IEnumerable<DataType> expectedPrimaryKeyDataTypes, string expectedLongestName)
        {
            IProvider sqlProvider = new SqlServer2008Provider();
            IProvider smoProvider = new SmoProvider();
            AssertAreEqual(sqlProvider, smoProvider, (Database)database);
        }

        private static void AssertAreEqual(IProvider sqlProvider, IProvider smoProvider, Database database)
        {
            MigrationReport report = MigrationReport.Create(database, string.Empty);
            Assert.IsEmpty(report.Error);
            var sqlTranslator = new CommandsToSqlTranslator(sqlProvider);
            var smoTranslator = new CommandsToSqlTranslator(smoProvider);
            var context = MockRepository.GenerateStub<IRuntimeContext>();
            ScriptComparer.AssertAreEqual(smoTranslator.TranslateToSql(database, context), sqlTranslator.TranslateToSql(database, context));
        }
    }
}