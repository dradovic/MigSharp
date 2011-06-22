using System;
using System.Data;

namespace MigSharp.Providers
{
    /// <summary>
    /// Declares the support of a specific data type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class SupportsAttribute : Attribute
    {
        private readonly DbType _dbType;

        /// <summary>
        /// Gets the supported data type.
        /// </summary>
        public DbType DbType { get { return _dbType; } }

        /// <summary>
        /// Gets or sets the maximum length for character data types or the maximum total number of decimal digits for numeric data types.
        /// </summary>
        public int MaximumSize { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of decimal digits that can be stored to the right of the decimal point. Scale is a value from 0 through <see cref="MaximumSize"/>.
        /// </summary>
        public int MaximumScale { get; set; }

        /// <summary>
        /// Indicates if the data type can be used in primary key columns. By default, this is false.
        /// </summary>
        public bool CanBeUsedAsPrimaryKey { get; set; }

        /// <summary>
        /// Indicates if the data type can be used for identity columns. By default, this is false.
        /// </summary>
        public bool CanBeUsedAsIdentity { get; set; }

        /// <summary>
        /// Gets or sets a warning message if there are any restrictions when using this <see cref="DbType"/>.
        /// The warning message is logged when executing a migration containing this data type.
        /// </summary>
        public string Warning { get; set; }

        /// <summary>
        /// Indicates if the data type can be scripted.
        /// </summary>
        public bool IsScriptable { get { return SqlScriptingHelper.IsScriptable(_dbType); } }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public SupportsAttribute(DbType dbType)
        {
            _dbType = dbType;
        }
    }
}