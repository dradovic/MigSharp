using System;
using System.Data;

using MigSharp.Process;
using MigSharp.Providers;

using NUnit.Framework;

using Rhino.Mocks;

namespace MigSharp.NUnit.Process
{
    [TestFixture, Category("Smoke")]
    public class MigrationStepTests
    {
        private const string TableName = "New Table";
        private const string ProviderInvariantName = "providerName";
        private const string FirstCommandText = "1st command";
        private const string SecondCommandText = "2nd command";

        [Test]
        public void TestUpgrading()
        {
            TestMigrating(MigrationDirection.Up, provider => provider.Expect(p => p.CreateTable(TableName, null, false)).IgnoreArguments().Return(new[] { FirstCommandText, SecondCommandText }));
        }

        [Test]
        public void TestDowngrading()
        {
            TestMigrating(MigrationDirection.Down, provider => provider.Expect(p => p.DropTable(TableName)).Return(new[] { FirstCommandText, SecondCommandText }));
        }

        private static void TestMigrating(MigrationDirection direction, Action<IProvider> setupExpectationOnProvider)
        {
            TestMigrationMetadata metadata = new TestMigrationMetadata();

            TestMigration migration = new TestMigration();
            IProvider provider = MockRepository.GenerateMock<IProvider>();
            setupExpectationOnProvider(provider);
            IProviderFactory providerFactory = MockRepository.GenerateStub<IProviderFactory>();
            IProviderMetadata providerMetadata;
            providerFactory.Expect(f => f.GetProvider(ProviderInvariantName, out providerMetadata)).Return(provider);

            IDbTransaction transaction = MockRepository.GenerateMock<IDbTransaction>();
            transaction.Expect(t => t.Commit());

            IDbConnection connection = MockRepository.GenerateMock<IDbConnection>();
            connection.Expect(c => c.State).Return(ConnectionState.Open).Repeat.Any();
            connection.Expect(c => c.BeginTransaction()).Return(transaction);

            IDbCommand firstCommand = MockRepository.GenerateMock<IDbCommand>();
            firstCommand.Expect(c => c.CommandText).SetPropertyWithArgument(FirstCommandText);
            firstCommand.Expect(c => c.ExecuteNonQuery()).Return(0);
            connection.Expect(c => c.CreateCommand()).Return(firstCommand).Repeat.Once();

            IDbCommand secondCommand = MockRepository.GenerateMock<IDbCommand>();
            secondCommand.Expect(c => c.CommandText).SetPropertyWithArgument(SecondCommandText);
            secondCommand.Expect(c => c.ExecuteNonQuery()).Return(0);
            connection.Expect(c => c.CreateCommand()).Return(secondCommand).Repeat.Once();

            connection.Expect(c => c.Dispose());
            IDbConnectionFactory connectionFactory = MockRepository.GenerateStub<IDbConnectionFactory>();
            connectionFactory.Expect(c => c.OpenConnection(null)).IgnoreArguments().Return(connection);
            MigrationStep step = new MigrationStep(migration, metadata, new ConnectionInfo("", ProviderInvariantName), providerFactory, connectionFactory);

            IVersioning versioning = MockRepository.GenerateMock<IVersioning>();
            versioning.Expect(v => v.Update(metadata, connection, transaction, direction));
#if DEBUG
            versioning.Expect(v => v.IsContained(metadata)).Return(direction == MigrationDirection.Up);
#endif
            step.Execute(versioning, direction);

            connection.VerifyAllExpectations();
            transaction.VerifyAllExpectations();
            provider.VerifyAllExpectations();
            firstCommand.VerifyAllExpectations();
            secondCommand.VerifyAllExpectations();
            versioning.VerifyAllExpectations();
        }

        private class TestMigration : IMigration
        {
            public void Up(IDatabase db)
            {
                db.CreateTable(TableName)
                    .WithPrimaryKeyColumn("Id", DbType.Int32);
            }

            public void Down(IDatabase db)
            {
                db.Tables[TableName].Drop();
            }
        }

        private class TestMigrationMetadata : IMigrationMetadata
        {
            public int Year { get { throw new NotSupportedException(); } }
            public int Month { get { throw new NotSupportedException(); } }
            public int Day { get { throw new NotSupportedException(); } }
            public int Hour { get { throw new NotSupportedException(); } }
            public int Minute { get { throw new NotSupportedException(); } }
            public int Second { get { throw new NotSupportedException(); } }
            public string Tag { get { throw new NotSupportedException(); } }
            public string ModuleName { get { throw new NotSupportedException(); } }
        }
    }
}