using System;
using System.Data;

namespace MigSharp.Process
{
    internal class DbVersion : IDbVersion
    {
        private readonly DbVersionDataSet _dataSet;

        private DbVersion(DbVersionDataSet dataSet)
        {
            _dataSet = dataSet;
        }

        public static DbVersion Create(ConnectionInfo connectionInfo)
        {
            var dataSet = new DbVersionDataSet();
            throw new NotImplementedException();
            return Create(dataSet);
        }

        internal static DbVersion Create(DbVersionDataSet dataSet)
        {
            return new DbVersion(dataSet);
        }

        public bool Includes(IMigrationMetaData metaData)
        {
            return _dataSet.DbVersion.FindByTimestamp(metaData.Timestamp) != null; // TODO: include Module
        }

        public void Update(IDbConnection connection, IMigration migration)
        {
            throw new NotImplementedException();
        }
    }
}