using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace MigSharp
{
    /// <summary>
    /// Represents a collection of database versions that should be supported for all migrations. Validation of migrations is performed
    /// against providers contained within this list.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    public class SupportedPlatforms : IEnumerable<DbPlatform>
    {
        private readonly List<DbPlatform> _platforms = new List<DbPlatform>();

        internal SupportedPlatforms()
        {
        }

        /// <summary>
        /// Adds or replaces a database platform whose <see cref="DbPlatform.MajorVersion"/> is taken to be the minimum requirement.
        /// </summary>
        /// <remarks>
        /// This method is added to support collection initializers.
        /// </remarks>
        public void Add(DbPlatform platform)
        {
            AddOrReplaceMinimumRequirement(platform);
        }

        /// <summary>
        /// Adds or replaces a database platform whose <see cref="DbPlatform.MajorVersion"/> is taken to be the minimum requirement.
        /// </summary>
        public void AddOrReplaceMinimumRequirement(DbPlatform platform)
        {
            _platforms.RemoveAll(p => p.Platform == platform.Platform && p.Driver == platform.Driver);
            _platforms.Add(platform);
        }

        /// <summary>
        /// Removes platforms by <see cref="Platform"/>.
        /// </summary>
        public void RemoveAll(Platform platform)
        {
            _platforms.RemoveAll(p => p.Platform == platform);
        }

        /// <summary>
        /// Removes platforms by <see cref="Driver"/>.
        /// </summary>
        public void RemoveAll(Driver driver)
        {
            _platforms.RemoveAll(p => p.Driver == driver);
        }

        /// <summary>
        /// Sets the collection of supported providers to <paramref name="names"/>.
        /// </summary>
        public void Set(IEnumerable<DbPlatform> names)
        {
            Clear();
            _platforms.AddRange(names);
        }

        /// <summary>
        /// Clears the collection.
        /// </summary>
        public void Clear()
        {
            _platforms.Clear();
        }

#pragma warning disable 1591

        public IEnumerator<DbPlatform> GetEnumerator()
        {
            return _platforms.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

#pragma warning restore 1591
    }
}