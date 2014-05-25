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
            DataType type6 = new DataType(DbType.AnsiStringFixedLength);
            DataType type7 = new DataType(DbType.AnsiStringFixedLength);
            DataType type8 = new DataType(DbType.AnsiStringFixedLength, 0);
            DataType type9 = new DataType(DbType.AnsiStringFixedLength, 0);
            Assert.IsTrue(type1 == type2);
            Assert.IsTrue(type1 != type3);
            Assert.IsTrue(type1 != type4);
            Assert.IsTrue(type1 != type5);
            Assert.IsTrue(type6 == type7);
            Assert.IsTrue(type1 != type7);
            Assert.IsTrue(type8 == type9);
            Assert.IsTrue(type7 != type8);
        }
    }
}