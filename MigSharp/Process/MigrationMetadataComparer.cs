using System.Collections.Generic;

namespace MigSharp.Process
{
    internal class MigrationMetadataComparer : IEqualityComparer<IMigrationMetadata>
    {
        public bool Equals(IMigrationMetadata x, IMigrationMetadata y)
        {
            return x.Timestamp == y.Timestamp && x.ModuleName == y.ModuleName;
        }

        public int GetHashCode(IMigrationMetadata obj)
        {
            return obj.Timestamp.GetHashCode() ^ obj.ModuleName.GetHashCode();
        }
    }
}