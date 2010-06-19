using MigSharp.Core;
using MigSharp.Core.Entities;
using MigSharp.NUnit;
using MigSharp.Providers;

using NUnit.Framework;

using Rhino.Mocks;

namespace MigSharp.SqlServer.NUnit
{
    [TestFixture, Category("SqlServer")]
    public class SqlServerProviderVsSmoTests
    {
        [Test, TestCaseSource(typeof(TestCaseGenerator), "GetDatabaseCases")]
        public void TestDatabaseCases(IDatabase database)
        {
            IProvider sqlProvider = new SqlServerProvider();
            IProvider smoProvider = new SmoProvider();
            AssertAreEqual(sqlProvider, smoProvider, (Database)database);
        }

        private static void AssertAreEqual(IProvider sqlProvider, IProvider smoProvider, Database database)
        {
            var metaData = MockRepository.GenerateStub<IProviderMetadata>();
            var sqlScripter = new CommandScripter(sqlProvider, metaData);
            var smoScripter = new CommandScripter(smoProvider, metaData);
            ScriptComparer.AssertAreEqual(smoScripter.GetCommandTexts(database), sqlScripter.GetCommandTexts(database));
        }
    }
}