using System.Data;

namespace MigSharp
{
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