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
    internal class Validator
    {
        private readonly MigrationOptions _options;

        public Validator(MigrationOptions options)
        {
            _options = options;
        }

        public void Validate(IEnumerable<IMigrationReporter> reporters, out string errors, out string warnings)
        {
            DateTime start = DateTime.Now;

            int numberOfProviders = 0;
            var errorMessages = new List<string>();
            var warningMessages = new List<ValidationWarning>();
            foreach (string providerName in _options.SupportedProviders.Names)
            {
                IProviderMetadata providerMetadata;
                IProvider provider = _options.SupportedProviders.GetProvider(providerName, out providerMetadata);
                List<SupportsAttribute> supportsAttributes = provider.GetSupportsAttributes().ToList();
                IEnumerable<UnsupportedMethod> unsupportedMethods = provider.GetUnsupportedMethods();

                var context = new MigrationContext(providerMetadata);
                foreach (IMigrationReporter reporter in reporters)
                {
                    IMigrationReport report = reporter.Report(context);
                    Validate(providerMetadata, supportsAttributes, unsupportedMethods, report, warningMessages, errorMessages);
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
                    providerMetadata.Name,
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
                        providerMetadata.Name));
                    continue;
                }
                // post-condition: the data type is supported
                
                // check if OfSize was specified correctly
                bool sizeIsSpecified = dataType.Size > 0;
                bool scaleIsSpecified = dataType.Scale > 0;
                SupportsAttribute attribute = attributes.Find(a => !(a.MaximumSize > 0 ^ sizeIsSpecified) && !(a.MaximumScale > 0 ^ scaleIsSpecified));
                if (attribute == null)
                {
                    errorMessages.Add(string.Format(CultureInfo.CurrentCulture,
                        "Migration '{0}' uses the data type '{1}' with a {2}zero size and a {3}zero scale which is not supported by '{4}'.",
                        report.MigrationName,
                        dataType,
                        sizeIsSpecified ? "non-" : string.Empty,
                        scaleIsSpecified ? "non-" : string.Empty,
                        providerMetadata.Name));
                    continue;
                }
                // post-condition: the data type is supported and OfSize was specified correctly

                // check other properties
                if (report.PrimaryKeyDataTypes.Contains(dataType) && !attribute.CanBeUsedAsPrimaryKey)
                {
                    errorMessages.Add(string.Format(CultureInfo.CurrentCulture,
                        "Migration '{0}' uses the data type '{1}' for a primary key which is not supported by '{2}'.",
                        report.MigrationName,
                        dataType,
                        providerMetadata.Name));
                }
                if (report.IdentityDataTypes.Contains(dataType) && !attribute.CanBeUsedAsIdentity)
                {
                    errorMessages.Add(string.Format(CultureInfo.CurrentCulture,
                        "Migration '{0}' uses the data type '{1}' for an identity column which is not supported by '{2}'.",
                        report.MigrationName,
                        dataType,
                        providerMetadata.Name));
                }
                if (attribute.MaximumSize > 0 && dataType.Size > attribute.MaximumSize)
                {
                    errorMessages.Add(string.Format(CultureInfo.CurrentCulture,
                        "Migration '{0}' uses the data type '{1}' which exceeds the maximum size of {2} supported by '{3}'.",
                        report.MigrationName,
                        dataType,
                        attribute.MaximumSize,
                        providerMetadata.Name));
                }
                if (attribute.MaximumScale > 0 && dataType.Scale > attribute.MaximumScale)
                {
                    errorMessages.Add(string.Format(CultureInfo.CurrentCulture,
                        "Migration '{0}' uses the data type '{1}' which exceeds the maximum scale of {2} supported by '{3}'.",
                        report.MigrationName,
                        dataType,
                        attribute.MaximumScale,
                        providerMetadata.Name));
                }
                if (!string.IsNullOrEmpty(attribute.Warning))
                {
                    warningMessages.Add(new ValidationWarning(report.MigrationName, dataType, providerMetadata.Name, attribute.Warning));
                }
                if (providerMetadata.InvariantName == "System.Data.Odbc") // ODBC specific warnings
                {
                    if (dataType.DbType == DbType.Int64)
                    {
                        warningMessages.Add(new ValidationWarning(report.MigrationName, dataType, providerMetadata.Name, "Int64 is not supported for DbParameters with ODBC; requires calling ToString to directly inline the value in the CommandText."));
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
                    providerMetadata.Name,
                    method.Message));
            }

            // filter suppressed warnings
            warningMessages.RemoveAll(WarningIsSuppressed);
        }

        private bool WarningIsSuppressed(ValidationWarning warning)
        {
            return _options.IsWarningSuppressed(warning.ProviderName, warning.DataType.DbType, warning.DataType.Size, warning.DataType.Scale);
        }
    }
}