using System;
using System.Globalization;

using MigSharp.Providers;

namespace MigSharp.Process
{
    internal class ValidationWarning
    {
        private readonly string _migrationName;
        private readonly DataType _dataType;
        private readonly string _providerName;
        private readonly string _warning;

        public DataType DataType { get { return _dataType; } }
        public string ProviderName { get { return _providerName; } }

        public string Message
        {
            get
            {
                return String.Format(CultureInfo.CurrentCulture,
                    "Migration '{0}' uses the data type '{1}' which is not fully supported by '{2}': {3}",
                    _migrationName,
                    _dataType,
                    _providerName,
                    _warning);
            }
        }

        public ValidationWarning(string migrationName, DataType dataType, string providerName, string warning)
        {
            _migrationName = migrationName;
            _dataType = dataType;
            _providerName = providerName;
            _warning = warning;
        }
    }
}