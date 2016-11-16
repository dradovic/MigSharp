using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;

using MigSharp.Core;
using MigSharp.Providers;

namespace MigSharp.Process
{
    /// <summary>
    /// Validates <see cref="IMigrationReport"/>s against the list of supported providers.
    /// </summary>
    internal class Validator : IValidator
    {
        private readonly IEnumerable<ProviderInfo> _providers;
        private readonly DbAltererOptions _options;

        public Validator(IEnumerable<ProviderInfo> providers, DbAltererOptions options)
        {
            _providers = providers;
            _options = options;
        }

        public void Validate(IEnumerable<IMigrationReporter> reporters, out string errors, out string warnings)
        {
            DateTime start = DateTime.Now;

            int numberOfProviders = 0;
            var errorMessages = new List<string>();
            var warningMessages = new List<ValidationWarning>();
            foreach (ProviderInfo info in _providers)
            {
                List<SupportsAttribute> supportsAttributes = info.Provider.GetSupportsAttributes().ToList();
                IEnumerable<UnsupportedMethod> unsupportedMethods = info.Provider.GetUnsupportedMethods();

                foreach (IMigrationReporter reporter in reporters)
                {
                    var context = new MigrationContext(info.Metadata, reporter.StepMetadata);
                    IMigrationReport report = reporter.Report(context);
                    Validate(info.Metadata, supportsAttributes, unsupportedMethods, report, warningMessages, errorMessages);
                }
                numberOfProviders++;
            }
            errors = string.Join(Environment.NewLine, errorMessages.Distinct().ToArray());
            warnings = string.Join(Environment.NewLine, warningMessages.Select(w => w.Message).Distinct().ToArray());

            Log.Verbose(LogCategory.Performance, "Validating migrations against {0} provider(s) took {1}s", numberOfProviders, (DateTime.Now - start).TotalSeconds);
        }

        private void Validate(IProviderMetadata providerMetadata, IEnumerable<SupportsAttribute> supportsAttributes, IEnumerable<UnsupportedMethod> unsupportedMethods, IMigrationReport report, List<ValidationWarning> warningMessages, List<string> errorMessages)
        {
            if (!string.IsNullOrEmpty(report.Error))
            {
                errorMessages.Add(string.Format(CultureInfo.InvariantCulture, "Error in migration '{0}': {1}", report.MigrationName, report.Error));
            }

            // check created object name lengths
            if (providerMetadata.MaximumDbObjectNameLength > 0 && !string.IsNullOrEmpty(report.LongestName) &&
                providerMetadata.MaximumDbObjectNameLength < report.LongestName.Length)
            {
                errorMessages.Add(string.Format(CultureInfo.CurrentCulture,
                    "Migration '{0}' contains object names that are longer than what is supported by '{1}' ('{2}': {3}, supported: {4}).",
                    report.MigrationName,
                    providerMetadata.GetPlatform(),
                    report.LongestName,
                    report.LongestName.Length,
                    providerMetadata.MaximumDbObjectNameLength));
            }

            // check used data types
            foreach (DataType dataType in report.DataTypes)
            {
                DbType dbType = dataType.DbType;
                List<SupportsAttribute> attributes = supportsAttributes.Where(a => a.DbType == dbType).ToList();
                if (attributes.Count == 0)
                {
                    errorMessages.Add(string.Format(CultureInfo.CurrentCulture,
                        "Migration '{0}' uses the data type '{1}' which is not supported by '{2}'.",
                        report.MigrationName,
                        dataType,
                        providerMetadata.GetPlatform()));
                    continue;
                }
                // post-condition: the data type is supported
                
                // check if OfSize was specified correctly
                SupportsAttribute attribute = attributes.Find(a => !(a.MaximumSize > 0 ^ dataType.Size.HasValue) && !(a.MaximumScale > 0 ^ dataType.Scale.HasValue));
                if (attribute == null)
                {
                    errorMessages.Add(string.Format(CultureInfo.CurrentCulture,
                        "Migration '{0}' uses the data type '{1}' which is not supported by '{2}'.",
                        report.MigrationName,
                        dataType,
                        providerMetadata.GetPlatform()));
                    continue;
                }
                // post-condition: the data type is supported and OfSize was specified with the correct number of parameters

                // check other properties
                if (report.PrimaryKeyDataTypes.Contains(dataType) && !attribute.CanBeUsedAsPrimaryKey)
                {
                    errorMessages.Add(string.Format(CultureInfo.CurrentCulture,
                        "Migration '{0}' uses the data type '{1}' for a primary key which is not supported by '{2}'.",
                        report.MigrationName,
                        dataType,
                        providerMetadata.GetPlatform()));
                }
                if (report.IdentityDataTypes.Contains(dataType) && !attribute.CanBeUsedAsIdentity)
                {
                    errorMessages.Add(string.Format(CultureInfo.CurrentCulture,
                        "Migration '{0}' uses the data type '{1}' for an identity column which is not supported by '{2}'.",
                        report.MigrationName,
                        dataType,
                        providerMetadata.GetPlatform()));
                }
                if (attribute.MaximumSize > 0 && dataType.Size > attribute.MaximumSize)
                {
                    errorMessages.Add(string.Format(CultureInfo.CurrentCulture,
                        "Migration '{0}' uses the data type '{1}' which exceeds the maximum size of {2} supported by '{3}'.",
                        report.MigrationName,
                        dataType,
                        attribute.MaximumSize,
                        providerMetadata.GetPlatform()));
                }
                if (attribute.MaximumScale > 0 && dataType.Scale > attribute.MaximumScale)
                {
                    errorMessages.Add(string.Format(CultureInfo.CurrentCulture,
                        "Migration '{0}' uses the data type '{1}' which exceeds the maximum scale of {2} supported by '{3}'.",
                        report.MigrationName,
                        dataType,
                        attribute.MaximumScale,
                        providerMetadata.GetPlatform()));
                }
                if (!string.IsNullOrEmpty(attribute.Warning))
                {
                    warningMessages.Add(new ValidationWarning(report.MigrationName, dataType, providerMetadata.GetPlatform(), attribute.Warning));
                }
                if (providerMetadata.InvariantName == "System.Data.Odbc") // ODBC specific warnings
                {
                    if (dataType.DbType == DbType.Int64)
                    {
                        warningMessages.Add(new ValidationWarning(report.MigrationName, dataType, providerMetadata.GetPlatform(), "Int64 is not supported for DbParameters with ODBC; requires calling ToString to directly inline the value in the CommandText."));
                    }
                }
            }

            // check used methods
            foreach (UnsupportedMethod method in unsupportedMethods
                .Join(report.Methods, um => um.Name, m => m, (um, m) => um))
            {
                errorMessages.Add(string.Format(CultureInfo.CurrentCulture,
                    "Migration '{0}' calls the '{1}' method which is not supported by '{2}': {3}",
                    report.MigrationName,
                    method.Name,
                    providerMetadata.GetPlatform(),
                    method.Message));
            }

            // filter suppressed warnings
            warningMessages.RemoveAll(WarningIsSuppressed);
        }

        private bool WarningIsSuppressed(ValidationWarning warning)
        {
            return _options.IsWarningSuppressed(warning.DbPlatform, warning.DataType.DbType, warning.DataType.Size, warning.DataType.Scale);
        }
    }
}