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
            var step = new MigrationStep(new BootstrapMigration(), new BootstrapMetaData(), connectionInfo, providerFactory, connectionFactory);
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

        public void Update(IDbConnection connection, IMigrationMetaData metaData)
        {
            if (metaData is BootstrapMetaData) return;

            _dataSet.DbVersion.AddDbVersionRow(metaData.Timestamp(), metaData.Tag, metaData.Module);
        }

        private class BootstrapMigration : IMigration
        {
            public void Up(IDatabase db)
            {
                db.CreateTable(TableName)
                    .WithPrimaryKeyColumn("Timestamp", DbType.DateTime)
                    .WithPrimaryKeyColumn("Module", DbType.StringFixedLength).OfLength(250)
                    .WithNullableColumn("Tag", DbType.String);
                // TODO: .IfNotExists();
            }
        }

        private class BootstrapMetaData : IMigrationMetaData
        {
            public int Year { get { throw new NotSupportedException(); } }
            public int Month { get { throw new NotSupportedException(); } }
            public int Day { get { throw new NotSupportedException(); } }
            public int Hour { get { throw new NotSupportedException(); } }
            public int Minute { get { throw new NotSupportedException(); } }
            public int Second { get { throw new NotSupportedException(); } }
            public string Tag { get { throw new NotSupportedException(); } }
            public string Module { get { throw new NotSupportedException(); } }
        }
    }
}