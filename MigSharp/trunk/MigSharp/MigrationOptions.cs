using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;

using MigSharp.Core;

namespace MigSharp
{
    /// <summary>
    /// Use this class to configure the behaviour of the <see cref="Migrator"/>.
    /// </summary>
    public class MigrationOptions
    {
        private string _versioningTableName = "MigSharp";
        /// <summary>
        /// Gets or sets the table name of the versioning table.
        /// </summary>
        public string VersioningTableName
        {
            get { return _versioningTableName; }
            set
            {
                if (string.IsNullOrEmpty(value)) throw new ArgumentException("The versioning table name cannot be empty.");
                _versioningTableName = value;
            }
        }

        private SupportedProviders _supportedProviders = new SupportedProviders();
        /// <summary>
        /// Gets the providers that should be supported for all migrations. Compatibility validation of migrations is performed
        /// against the providers in this collection.
        /// </summary>
        public SupportedProviders SupportedProviders { get { return _supportedProviders; } internal set { _supportedProviders = value; } }

        private Predicate<string> _moduleSelector = n => true; // select all modules by default
        /// <summary>
        /// Gets or sets a function that selects the module based on its name. Only migrations for this module will be executed.
        /// </summary>
        public Predicate<string> ModuleSelector
        {
            get { return _moduleSelector; }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                _moduleSelector = value;
            }
        }

        private readonly List<Suppression> _warningSuppressions = new List<Suppression>();

        /// <summary>
        /// Initializes an instance of default options.
        /// </summary>
        public MigrationOptions()
        {
        }

        /// <summary>
        /// Suppresses validation warnings for the provider called <paramref name="providerName"/> and the data type <paramref name="type"/> under the <paramref name="condition"/>.
        /// </summary>
        public void SuppressWarning(string providerName, DbType type, SuppressCondition condition)
        {
            _warningSuppressions.Add(new Suppression(providerName, type, condition));
        }

        internal bool IsWarningSuppressed(string providerName, DbType type, int size, int scale)
        {
            foreach (Suppression suppression in _warningSuppressions
                .Where(s => s.ProviderName == providerName && s.Type == type))
            {

                switch (suppression.Condition)
                {
                    case SuppressCondition.WhenSpecifiedWithoutSize:
                        if (size == 0) return true;
                        break;
                    case SuppressCondition.WhenSpecifiedWithSize:
                        if (size > 0 && scale == 0) return true;
                        break;
                    case SuppressCondition.WhenSpecifiedWithSizeAndScale:
                        if (size > 0 && scale > 0) return true;
                        break;
                    case SuppressCondition.Always:
                        return true;
                    default:
                        continue;
                }
            }
            return false;
        }

        /// <summary>
        /// Initializes options that select migrations for specific module only.
        /// </summary>
        /// <param name="moduleName">The name of the selected module. Only migrations for this module will be executed.</param>
        public MigrationOptions(string moduleName)
        {
            ModuleSelector = n => n == moduleName;
        }

        #region Static Options

        ///<summary>
        /// Sets the level of general information being traced.
        ///</summary>
        public static void SetGeneralTraceLevel(SourceLevels sourceLevels)
        {
            Log.SetTraceLevel(LogCategory.General, sourceLevels);
        }

        ///<summary>
        /// Sets the level of SQL information being traced.
        ///</summary>
        public static void SetSqlTraceLevel(SourceLevels sourceLevels) // signature used in a Wiki example
        {
            Log.SetTraceLevel(LogCategory.Sql, sourceLevels);
        }

        ///<summary>
        /// Sets the level of performance information being traced.
        ///</summary>
        public static void SetPerformanceTraceLevel(SourceLevels sourceLevels)
        {
            Log.SetTraceLevel(LogCategory.Performance, sourceLevels);
        }

        #endregion

        private class Suppression
        {
            private readonly string _providerName;
            private readonly DbType _type;
            private readonly SuppressCondition _condition;

            public string ProviderName { get { return _providerName; } }
            public DbType Type { get { return _type; } }
            public SuppressCondition Condition { get { return _condition; } }

            public Suppression(string providerName, DbType type, SuppressCondition condition)
            {
                _providerName = providerName;
                _type = type;
                _condition = condition;
            }
        }
    }

    /// <summary>
    /// Expresses under which circumstances a warning should be expressed for a given <see cref="DbType"/> and its OfSize parameters.
    /// </summary>
    public enum SuppressCondition
    {
        /// <summary>
        /// Suppresses all warnings for the specified <see cref="DbType"/>. Use diligently.
        /// </summary>
        Always = 0,

        /// <summary>
        /// Suppresses warnings for the specified <see cref="DbType"/> when it is used without a specified size.
        /// </summary>
        WhenSpecifiedWithoutSize = 1,

        /// <summary>
        /// Suppresses warnings for the specified <see cref="DbType"/> when it is used with a specified size.
        /// </summary>
        WhenSpecifiedWithSize = 2,

        /// <summary>
        /// Suppresses warnings for the specified <see cref="DbType"/> when it is used with a specified size and a specified scale.
        /// </summary>
        WhenSpecifiedWithSizeAndScale = 3,
    }
}