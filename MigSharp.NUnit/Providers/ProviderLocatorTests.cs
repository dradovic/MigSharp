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
            var locator = new ProviderLocator(new ProviderRegistry());
            string[] providerTypeNames = locator.GetAllForMinimumRequirement(DbPlatform.SqlServer2012).Select(i => i.Provider).Select(p => p.GetType().Name).ToArray();
            CollectionAssert.AreEquivalent(new[] { "SqlServer2012Provider" }, providerTypeNames);
        }

        [Test]
        public void TestGetAllForMinimumRequirementSelectsTheLatestIfNotReachable()
        {
            var locator = new ProviderLocator(new ProviderRegistry());
            string[] providerTypeNames = locator.GetAllForMinimumRequirement(DbPlatform.SqlServer2014).Select(i => i.Provider).Select(p => p.GetType().Name).ToArray();
            CollectionAssert.AreEquivalent(new[] { "SqlServer2012Provider" }, providerTypeNames);            
        }

        [Test]
        public void TestGetLatestWhenThereIsMatchingProvider()
        {
            var locator = new ProviderLocator(new ProviderRegistry());
            ProviderInfo info = locator.GetLatest(DbPlatform.SqlServer2012);
            Assert.AreEqual("SqlServer2012Provider", info.Provider.GetType().Name);
        }

        [Test]
        public void TestGetLatestWhenThereIsOnlyOlderProviders()
        {
            var locator = new ProviderLocator(new ProviderRegistry());
            ProviderInfo info = locator.GetLatest(DbPlatform.SqlServer2014);
            Assert.AreEqual("SqlServer2012Provider", info.Provider.GetType().Name);
        }

        [Test]
        public void TestGetLatestWhenThereIsNoMatchingProviders()
        {
            var locator = new ProviderLocator(new ProviderRegistry());
            Assert.That(() => locator.GetLatest(new DbPlatform(Platform.SqlServer, 8)), Throws.Exception);
        }

        [Test]
        public void TestGetExactly()
        {
            var locator = new ProviderLocator(new ProviderRegistry());
            ProviderInfo info = locator.GetExactly(DbPlatform.SqlServer2012);
            Assert.AreEqual("SqlServer2012Provider", info.Provider.GetType().Name);
        }

        [Test]
        public void TestGetExactlyThrowsIfNoMatchingProviderIsFound()
        {
            var locator = new ProviderLocator(new ProviderRegistry());
            Assert.That(() => locator.GetExactly(DbPlatform.SqlServer2014), Throws.Exception);
        }
    }
}