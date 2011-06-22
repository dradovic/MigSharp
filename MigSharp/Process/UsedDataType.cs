using System;

using MigSharp.Providers;

namespace MigSharp.Process
{
    internal struct UsedDataType : IEquatable<UsedDataType>
    {
        private readonly DataType _dataType;
        private readonly bool _usedAsPrimaryKey;
        private readonly bool _usedAsIdentity;

        public DataType DataType { get { return _dataType; } }
        public bool UsedAsPrimaryKey { get { return _usedAsPrimaryKey; } }
        public bool UsedAsIdentity { get { return _usedAsIdentity; } }

        public UsedDataType(DataType dataType, bool usedAsPrimaryKey, bool usedAsIdentity)
        {
            _dataType = dataType;
            _usedAsPrimaryKey = usedAsPrimaryKey;
            _usedAsIdentity = usedAsIdentity;
        }

        #region Equality And Hashing

        public bool Equals(UsedDataType other)
        {
            return (this == other);
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (GetType() != obj.GetType()) return false;

            return (this == (UsedDataType)obj);
        }

        public static bool operator ==(UsedDataType left, UsedDataType right)
        {
            return
                left._dataType == right._dataType &&
                left._usedAsPrimaryKey == right._usedAsPrimaryKey &&
                left._usedAsIdentity == right._usedAsIdentity;
        }

        public static bool operator !=(UsedDataType left, UsedDataType right)
        {
            return !(left == right);
        }

        public override int GetHashCode()
        {
            return _dataType.GetHashCode() ^ _usedAsPrimaryKey.GetHashCode() ^ _usedAsIdentity.GetHashCode();
        }

        #endregion
    }
}