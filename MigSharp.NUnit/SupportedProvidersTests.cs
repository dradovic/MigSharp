using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace MigSharp.NUnit
{
    [TestFixture, Category("smoke")]
    public class SupportedProvidersTests
    {
        [Test]
        public void VerifyDefaultSupportedProviders()
        {
            var expectedProviderNames = GetExpectedProviderNames();
            var providers = new SupportedProviders();
            CollectionAssert.AreEquivalent(expectedProviderNames, providers.Names);
        }

        [Test]
        public void VerifyRemoveAllOdbcRemovesAllOdbcProviders()
        {
            var expectedProviderNames = GetExpectedProviderNames().Where(n => !n.EndsWith("Odbc", StringComparison.OrdinalIgnoreCase));
            CollectionAssert.IsNotEmpty(expectedProviderNames);

            var providers = new SupportedProviders();
            providers.RemoveAllOdbc();
            CollectionAssert.AreEquivalent(expectedProviderNames, providers.Names);
        }

        private static IEnumerable<string> GetExpectedProviderNames()
        {
            var result = new List<string>(typeof(ProviderNames)
                                              .GetFields()
                                              .Where(f => f.IsStatic)
                                              .Select(f => (string)f.GetValue(null)));
            CollectionAssert.IsNotEmpty(result);
            return result;
        }
    }
}