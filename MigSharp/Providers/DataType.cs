using System;
using System.Data;
using System.Globalization;

namespace MigSharp.Providers
{
    /// <summary>
    /// Represents a SQL data type.
    /// </summary>
    public struct DataType : IEquatable<DataType>
    {
        private readonly DbType _dbType;
        private readonly int? _size;
        private readonly int? _scale;

        /// <summary>
        /// Gets the associated <see cref="DbType"/>.
        /// </summary>
        public DbType DbType { get { return _dbType; } }

        /// <summary>
        /// Gets the length for character data types, or the maximum total number of decimal digits for numeric data types and <see cref="System.Data.DbType.DateTime2"/>.
        /// </summary>
        public int? Size { get { return _size; } }

        /// <summary>
        /// Gets the maximum number of decimal digits that can be stored to the right of the decimal point. Scale is a value from 0 through <see cref="Size"/>.
        /// </summary>
        public int? Scale { get { return _scale; } }

        internal DataType(DbType dbType, int? size, int? scale)
        {
            _dbType = dbType;
            _size = size;
            _scale = scale;
        }

        internal DataType(DbType dbType, int? size)
            : this(dbType, size, null)
        {
        }

        internal DataType(DbType dbType)
            : this(dbType, null, null)
        {
        }

        #region Equality And Hashing

        public bool Equals(DataType other)
        {
            return (this == other);
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (GetType() != obj.GetType()) return false;

            return (this == (DataType)obj);
        }

#pragma warning disable 1591
        public static bool operator ==(DataType left, DataType right)
#pragma warning restore 1591
        {
            return
                left._dbType == right._dbType &&
                left._size == right._size &&
                left._scale == right._scale;
        }

#pragma warning disable 1591
        public static bool operator !=(DataType left, DataType right)
#pragma warning restore 1591
        {
            return !(left == right);
        }

        public override int GetHashCode()
        {
            return _dbType.GetHashCode() ^ _size.GetHashCode() ^ _scale.GetHashCode();
        }

        #endregion

        /// <summary>
        /// Used in validation messages and for debugging.
        /// </summary>
        public override string ToString()
        {
            if (_scale.HasValue)
            {
                return string.Format(CultureInfo.InvariantCulture, "{0}({1},{2})", _dbType, _size.HasValue ? (object)_size : "null", _scale);
            }
            else if (_size.HasValue)
            {
                return string.Format(CultureInfo.InvariantCulture, "{0}({1})", _dbType, _size);                
            }
            else
            {
                return string.Format(CultureInfo.InvariantCulture, "{0}", _dbType);                                
            }
        }
    }
}