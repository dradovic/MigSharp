using System.Collections.Generic;
using System.Data;
using System.Linq;
using MigSharp.Process;

namespace MigSharp
{
    /// <summary>
    /// Contains all options available when altering a database schema.
    /// </summary>
    public class DbAltererOptions
    {
        private SupportedPlatforms _supportedPlatforms = new SupportedPlatforms();
        private readonly List<Suppression> _warningSuppressions = new List<Suppression>();

        /// <summary>
        /// Initializes default options.
        /// </summary>
        public DbAltererOptions()
        {
            Validate = true;
        }

        /// <summary>
        /// Gets the providers that should be supported for all schema altering operations. Compatibility validation is performed
        /// against the providers in this collection.
        /// </summary>
        public SupportedPlatforms SupportedPlatforms { get { return _supportedPlatforms; } internal set { _supportedPlatforms = value; } }

        /// <summary>
        /// Gets or sets an indication if performed schema altering operations are validated against the capabilities of the <see cref="SupportedPlatforms"/>. Default is true.
        /// </summary>
        public bool Validate { get; set; }

        /// <summary>
        /// Suppresses validation warnings for <paramref name="dbPlatform"/> and the data type <paramref name="type"/> under the <paramref name="condition"/>.
        /// </summary>
        public void SuppressWarning(DbPlatform dbPlatform, DbType type, SuppressCondition condition)
        {
            _warningSuppressions.Add(new Suppression(dbPlatform, type, condition));
        }

        internal bool IsWarningSuppressed(DbPlatform dbPlatform, DbType type, int? size, int? scale)
        {
            foreach (Suppression suppression in _warningSuppressions
                .Where(s => s.DbPlatform.Matches(dbPlatform) && s.Type == type))
            {
                switch (suppression.Condition)
                {
                    case SuppressCondition.WhenSpecifiedWithoutSize:
                        if (!size.HasValue) return true;
                        break;
                    case SuppressCondition.WhenSpecifiedWithSize:
                        if (size.HasValue && !scale.HasValue) return true;
                        break;
                    case SuppressCondition.WhenSpecifiedWithSizeAndScale:
                        if (size.HasValue && scale.HasValue) return true;
                        break;
                    case SuppressCondition.Always:
                        return true;
                    default:
                        continue;
                }
            }
            return false;
        }

        private class Suppression
        {
            private readonly DbPlatform _dbPlatform;
            private readonly DbType _type;
            private readonly SuppressCondition _condition;

            public DbPlatform DbPlatform { get { return _dbPlatform; } }
            public DbType Type { get { return _type; } }
            public SuppressCondition Condition { get { return _condition; } }

            public Suppression(DbPlatform dbPlatform, DbType type, SuppressCondition condition)
            {
                _dbPlatform = dbPlatform;
                _type = type;
                _condition = condition;
            }
        }

        internal virtual ScriptingOptions GetScriptingOptions()
        {
            return new ScriptingOptions(ScriptingMode.ExecuteOnly, null);
        }
    }
}