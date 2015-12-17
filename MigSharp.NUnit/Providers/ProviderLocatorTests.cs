using MigSharp.Providers;
using NUnit.Framework;
using System.Linq;

namespace MigSharp.NUnit.Providers
{
    [TestFixture, Category("smoke")]
    public class ProviderLocatorTests
    {
        [Test]
        public void TestGetAllForMinimumRequirement()
        {
            var locator = new ProviderLocator(new ProviderFactory());
            string[] providerTypeNames = locator.GetAllForMinimumRequirement(DbPlatform.SqlServer2008).Select(i => i.Provider).Select(p => p.GetType().Name).ToArray();
            CollectionAssert.AreEquivalent(new[] { "SqlServer2008Provider", "SqlServer2012Provider" }, providerTypeNames);
        }

        [Test]
        public void TestGetAllForMinimumRequirementSelectsTheLatestIfNotReachable()
        {
            var locator = new ProviderLocator(new ProviderFactory());
            string[] providerTypeNames = locator.GetAllForMinimumRequirement(DbPlatform.SqlServer2014).Select(i => i.Provider).Select(p => p.GetType().Name).ToArray();
            CollectionAssert.AreEquivalent(new[] { "SqlServer2012Provider" }, providerTypeNames);            
        }

        [Test]
        public void TestGetLatestWhenThereIsMatchingProvider()
        {
            var locator = new ProviderLocator(new ProviderFactory());
            ProviderInfo info = locator.GetLatest(DbPlatform.SqlServer2008);
            Assert.AreEqual("SqlServer2008Provider", info.Provider.GetType().Name);
        }

        [Test]
        public void TestGetLatestWhenThereIsOnlyOlderProviders()
        {
            var locator = new ProviderLocator(new ProviderFactory());
            ProviderInfo info = locator.GetLatest(DbPlatform.SqlServer2014);
            Assert.AreEqual("SqlServer2012Provider", info.Provider.GetType().Name);
        }

        [Test, ExpectedException]
        public void TestGetLatestWhenThereIsNoMatchingProviders()
        {
            var locator = new ProviderLocator(new ProviderFactory());
            ProviderInfo info = locator.GetLatest(new DbPlatform(Platform.SqlServer, 8));
            Assert.AreEqual("SqlServer2000Provider", info.Provider.GetType().FullName);
        }

        [Test]
        public void TestGetExactly()
        {
            var locator = new ProviderLocator(new ProviderFactory());
            ProviderInfo info = locator.GetExactly(DbPlatform.SqlServer2008);
            Assert.AreEqual("SqlServer2008Provider", info.Provider.GetType().Name);
        }

        [Test, ExpectedException]
        public void TestGetExactlyThrowsIfNoMatchingProviderIsFound()
        {
            var locator = new ProviderLocator(new ProviderFactory());
            ProviderInfo info = locator.GetExactly(DbPlatform.SqlServer2014);
            Assert.AreEqual("SqlServer2014Provider", info.Provider.GetType().FullName);
        }
    }
}