using MigSharp.Core;
using MigSharp.Providers;
using MigSharp.Smo;

using NUnit.Framework;

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
            CommandScripter sqlScripter = new CommandScripter(sqlProvider);
            CommandScripter smoScripter = new CommandScripter(smoProvider);
            ScriptComparer.AssertAreEqual(smoScripter.GetCommandTexts(database), sqlScripter.GetCommandTexts(database));
        }
    }
}