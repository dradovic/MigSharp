using System;
using System.Data;
using System.Diagnostics;

using MigSharp.Providers;

namespace MigSharp.Process
{
    internal class DbVersion : IDbVersion
    {
        private readonly DbVersionDataSet _dataSet;

        internal static string TableName { get { return "DbVersion"; } }

        private DbVersion(DbVersionDataSet dataSet)
        {
            _dataSet = dataSet;
        }

        public static DbVersion Create(ConnectionInfo connectionInfo, IProviderFactory providerFactory, IDbConnectionFactory connectionFactory)
        {
            var dataSet = new DbVersionDataSet();
            Debug.Assert(dataSet.DbVersion.TableName == TableName);
            var dbVersion = new DbVersion(dataSet);
            var step = new MigrationStep(new BootstrapMigration(), connectionInfo, providerFactory, connectionFactory);
            step.Execute(dbVersion);
            return dbVersion;
        }

        internal static DbVersion Create(DbVersionDataSet dataSet)
        {
            return new DbVersion(dataSet);
        }

        public bool Includes(IMigrationMetaData metaData)
        {
            return _dataSet.DbVersion.FindByTimestampModule(metaData.Timestamp(), string.Empty) != null; // TODO: include Module instead of string.Empty
        }

        public void Update(IDbConnection connection, IMigration migration)
        {
            if (migration is BootstrapMigration) return;

            throw new NotImplementedException();
        }

        private class BootstrapMigration : IMigration
        {
            public void Up(IDatabase db)
            {
                db.CreateTable(TableName)
                    .WithPrimaryKeyColumn("Timestamp", DbType.DateTime)
                    .WithPrimaryKeyColumn("Module", DbType.String) // TODO: must be nullable
                    .WithNullableColumn("Tag", DbType.String);
                // TODO: .IfNotExists();
            }
        }
    }
}