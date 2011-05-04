using MigSharp.Core;

using NUnit.Framework;

namespace MigSharp.NUnit.Core
{
    [TestFixture, Category("smoke")]
    public class StringHelperTests
    {
        [Test]
        public void TestOnCollectionContainingNulls()
        {
            Assert.AreEqual("Longest", new [] { null, null, "Shorty", null, "Longest", null, "Sho" }.Longest());
        }

        [Test]
        public void TestOnEmptyCollections()
        {
            Assert.AreEqual(string.Empty, new string[] { }.Longest());
        }
    }
}