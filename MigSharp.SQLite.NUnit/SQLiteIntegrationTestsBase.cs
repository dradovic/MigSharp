using MigSharp.NUnit.Integration;
using NUnit.Framework;

namespace MigSharp.SQLite.NUnit
{
// ReSharper disable InconsistentNaming
    public abstract class SQLiteIntegrationTestsBase : IntegrationTestsBase
// ReSharper restore InconsistentNaming
    {
        protected override DbPlatform DbPlatform { get { return DbPlatform.SQLite3; } }

        [Test]
        public override void TestMigrationWithinTransactionScopeComplete()
        {
            // we don't execute this test yet since
            // TransactionScope is not fully supported
            // by this provider
        }

        [Test]
        public override void TestMigrationWithinTransactionScopeRollback()
        {
            // we don't execute this test yet since
            // TransactionScope is not fully supported
            // by this provider
        }
    }
}