using MigSharp.Core;
using MigSharp.Core.Entities;
using MigSharp.Providers;
using MigSharp.Smo;

using NUnit.Framework;

using Rhino.Mocks;

namespace MigSharp.NUnit.Provider
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
            var metaData = MockRepository.GenerateStub<IProviderMetaData>();
            var sqlScripter = new CommandScripter(sqlProvider, metaData);
            var smoScripter = new CommandScripter(smoProvider, metaData);
            ScriptComparer.AssertAreEqual(smoScripter.GetCommandTexts(database), sqlScripter.GetCommandTexts(database));
        }
    }
}