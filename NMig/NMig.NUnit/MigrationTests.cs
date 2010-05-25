using NMig.Core;
using NMig.Providers;

using NUnit.Framework;

namespace NMig.NUnit
{
    [TestFixture]
    public class MigrationTests
    {
        [Test]
        public void TestRenamingTable()
        {
            Database db = new Database();
            db.Tables["Customers"].Rename("Customer");
            IProvider provider = new SqlServerProvider();
            Scripter scripter = new Scripter(provider);
            string script = scripter.Script(db);
            Assert.AreEqual("sp_rename N'Customers', N'Customer'", script);
        }

        [Test]
        public void TestRenamingColumn()
        {
            Database db = new Database();
            db.Tables["S_AggregatorValues"].Columns["Val"].Rename("ValAbsoluteIncome");
            IProvider provider = new SqlServerProvider();
            Scripter scripter = new Scripter(provider);
            string script = scripter.Script(db);
            Assert.AreEqual("sp_rename N'Val', N'ValAbsoluteIncome', 'COLUMN'", script);
        }
    }
}