using System.Linq;
using NUnit.Framework;

namespace MigSharp.NUnit
{
    [TestFixture, Category("smoke")]
    public class SupportedPlatformsTests
    {
        [Test]
        public void TestAddOrReplace()
        {
            var supportedPlatforms = new SupportedPlatforms();
            CollectionAssert.IsEmpty(supportedPlatforms);
            supportedPlatforms.AddOrReplaceMinimumRequirement(DbPlatform.SqlServer2008);
            CollectionAssert.AreEquivalent(new[] { "SqlServer, Version: 10, Driver: AdoNet" }, supportedPlatforms.Select(n => n.ToString()).ToArray());
            supportedPlatforms.AddOrReplaceMinimumRequirement(DbPlatform.SqlServer2012);
            CollectionAssert.AreEquivalent(new[] { "SqlServer, Version: 11, Driver: AdoNet" }, supportedPlatforms.Select(n => n.ToString()).ToArray());
            supportedPlatforms.AddOrReplaceMinimumRequirement(new DbPlatform(Platform.SqlServer, 12, Driver.Odbc));
            CollectionAssert.AreEquivalent(new[]
                {
                    "SqlServer, Version: 11, Driver: AdoNet",
                    "SqlServer, Version: 12, Driver: Odbc"
                }, supportedPlatforms.Select(n => n.ToString()).ToArray());
        }
    }
}