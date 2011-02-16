using System.Data;

using MigSharp.Providers;

using NUnit.Framework;

namespace MigSharp.NUnit.Providers
{
    [TestFixture]
    public class DataTypeTests
    {
        [Test]
        public void TestEquals()
        {
            DataType type1 = new DataType(DbType.AnsiStringFixedLength, 20, 0);
            DataType type2 = new DataType(DbType.AnsiStringFixedLength, 20, 0);
            DataType type3 = new DataType(DbType.StringFixedLength, 20, 0);
            DataType type4 = new DataType(DbType.AnsiStringFixedLength, 0, 0);
            DataType type5 = new DataType(DbType.AnsiStringFixedLength, 0, 1);
            Assert.IsTrue(type1 == type2);
            Assert.IsTrue(type1 != type3);
            Assert.IsTrue(type1 != type4);
            Assert.IsTrue(type1 != type5);
        }
    }
}