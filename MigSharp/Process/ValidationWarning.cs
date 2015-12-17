using System;
using System.Globalization;

using MigSharp.Providers;

namespace MigSharp.Process
{
    internal class ValidationWarning
    {
        private readonly string _migrationName;
        private readonly DataType _dataType;
        private readonly DbPlatform _dbPlatform;
        private readonly string _warning;

        public DataType DataType { get { return _dataType; } }
        public DbPlatform DbPlatform { get { return _dbPlatform; } }

        public string Message
        {
            get
            {
                return String.Format(CultureInfo.CurrentCulture,
                    "Migration '{0}' uses the data type '{1}' which is not fully supported by '{2}': {3}",
                    _migrationName,
                    _dataType,
                    _dbPlatform,
                    _warning);
            }
        }

        public ValidationWarning(string migrationName, DataType dataType, DbPlatform dbPlatform, string warning)
        {
            _migrationName = migrationName;
            _dataType = dataType;
            _dbPlatform = dbPlatform;
            _warning = warning;
        }
    }
}