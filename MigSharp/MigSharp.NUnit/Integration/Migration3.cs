using System.Data;

using NUnit.Framework;

namespace MigSharp.NUnit.Integration
{
    [MigrationExport]
    internal class Migration3 : IMigration
    {
        public void Up(IDatabase db)
        {
            Assert.IsNotNull(db.Context);
            Assert.IsNotNull(db.Context.Connection);
            Assert.AreEqual(ConnectionState.Open, db.Context.Connection.State);
            Assert.IsNotNull(db.Context.Transaction);
        }
    }
}