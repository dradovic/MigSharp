using System.Data;

using MigSharp.Process;
using MigSharp.Providers;

using NUnit.Framework;

using Rhino.Mocks;

namespace MigSharp.NUnit.Process
{
    [TestFixture]
    public class MigrationStepTests
    {
        private const string TableName = "New Table";

        [Test, Category("Smoke")]
        public void TestExecute()
        {
            const string providerInvariantName = "providerName";
            const string firstCommandText = "1st command";
            const string secondCommandText = "2nd command";

            TestMigration migration = new TestMigration();
            IProvider provider = MockRepository.GenerateMock<IProvider>();
            provider.Expect(p => p.CreateTable(TableName, null)).IgnoreArguments().Return(new[] { firstCommandText, secondCommandText });
            IProviderFactory providerFactory = MockRepository.GenerateStub<IProviderFactory>();
            providerFactory.Expect(f => f.GetProvider(providerInvariantName)).Return(provider);

            IDbTransaction transaction = MockRepository.GenerateMock<IDbTransaction>();
            transaction.Expect(t => t.Commit());

            IDbConnection connection = MockRepository.GenerateMock<IDbConnection>();
            connection.Expect(c => c.State).Return(ConnectionState.Open).Repeat.Any();
            connection.Expect(c => c.BeginTransaction()).Return(transaction);

            IDbCommand firstCommand = MockRepository.GenerateMock<IDbCommand>();
            firstCommand.Expect(c => c.CommandText).SetPropertyWithArgument(firstCommandText);
            firstCommand.Expect(c => c.ExecuteNonQuery()).Return(0);
            connection.Expect(c => c.CreateCommand()).Return(firstCommand).Repeat.Once();

            IDbCommand secondCommand = MockRepository.GenerateMock<IDbCommand>();
            secondCommand.Expect(c => c.CommandText).SetPropertyWithArgument(secondCommandText);
            secondCommand.Expect(c => c.ExecuteNonQuery()).Return(0);
            connection.Expect(c => c.CreateCommand()).Return(secondCommand).Repeat.Once();

            connection.Expect(c => c.Dispose());
            IDbConnectionFactory connectionFactory = MockRepository.GenerateStub<IDbConnectionFactory>();
            connectionFactory.Expect(c => c.OpenConnection(null)).IgnoreArguments().Return(connection);
            MigrationStep step = new MigrationStep(migration, new ConnectionInfo("", providerInvariantName), providerFactory, connectionFactory);

            IDbVersion dbVersion = MockRepository.GenerateMock<IDbVersion>();
            dbVersion.Expect(v => v.Update(connection, migration));
            step.Execute(dbVersion);

            connection.VerifyAllExpectations();
            transaction.VerifyAllExpectations();
            provider.VerifyAllExpectations();
            firstCommand.VerifyAllExpectations();
            secondCommand.VerifyAllExpectations();
            dbVersion.VerifyAllExpectations();
        }

        private class TestMigration : IMigration
        {
            public void Up(IDatabase db)
            {
                db.CreateTable(TableName)
                    .WithPrimaryKeyColumn("Id", DbType.Int32);
            }
        }
    }
}