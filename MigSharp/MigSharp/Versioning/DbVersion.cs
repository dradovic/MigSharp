using System;
using System.Data.Common;

namespace MigSharp.Versioning
{
    internal class DbVersion : IDbVersion
    {
        private DbVersion()
        {
        }

        public static DbVersion Create(string connectionString)
        {
            return new DbVersion();
        }

        public bool Includes(IMigration migration)
        {
            throw new NotImplementedException();
        }

        public void Update(DbConnection connection, IMigration migration)
        {
            throw new NotImplementedException();
        }
    }
}