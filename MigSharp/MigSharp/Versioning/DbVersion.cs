using System;
using System.Data.Common;

namespace MigSharp.Versioning
{
    internal class DbVersion : IDbVersion
    {
        private readonly DbVersionDataSet _dataSet;

        private DbVersion(DbVersionDataSet dataSet)
        {
            _dataSet = dataSet;
        }

        public static DbVersion Create(string connectionString)
        {
            var dataSet = new DbVersionDataSet();
            return Create(dataSet);
        }

        internal static DbVersion Create(DbVersionDataSet dataSet)
        {
            return new DbVersion(dataSet);
        }

        public bool Includes(IMigrationMetaData metaData)
        {
            return _dataSet.DbVersion.FindByTimestamp(metaData.Timestamp) != null;
        }

        public void Update(DbConnection connection, IMigration migration)
        {
            throw new NotImplementedException();
        }
    }
}