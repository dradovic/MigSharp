using System.Data;
using System.Globalization;

namespace MigSharp.Providers
{
    /// <summary>
    /// Represents a SQL data type.
    /// </summary>
    public struct DataType
    {
        private readonly DbType _dbType;
        private readonly int _size;
        private readonly int _scale;

        public DbType DbType { get { return _dbType; } }

        /// <summary>
        /// Gets the length for character data types or the maximum total number of decimal digits for numeric data types.
        /// </summary>
        public int Size { get { return _size; } }

        /// <summary>
        /// Gets the maximum number of decimal digits that can be stored to the right of the decimal point. Scale is a value from 0 through <see cref="Size"/>.
        /// </summary>
        public int Scale { get { return _scale; } }

        /// <summary>
        /// Initializes a new instance of <see cref="DataType"/>.
        /// </summary>
        public DataType(DbType dbType, int size, int scale)
        {
            _dbType = dbType;
            _size = size;
            _scale = scale;
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

        public static bool operator ==(DataType left, DataType right)
        {
            return
                left._dbType == right._dbType &&
                left._size == right._size &&
                left._scale == right._scale;
        }

        public static bool operator !=(DataType left, DataType right)
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
            if (_scale > 0)
            {
                return string.Format(CultureInfo.InvariantCulture, "{0}({1},{2})", _dbType, _size, _scale);
            }
            else if (_size > 0)
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