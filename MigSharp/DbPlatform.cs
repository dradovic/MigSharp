using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace MigSharp
{
    /// <summary>
    /// Represents the specification of a database platform including its major version and driver used to access it.
    /// </summary>
    public class DbPlatform
    {
#pragma warning disable 1591
// ReSharper disable InconsistentNaming

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "DbPlatform is immutable.")]
        public static readonly DbPlatform MySql5 = new DbPlatform(Platform.MySql, 5);

        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "g")]
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "DbPlatform is immutable.")]
        public static readonly DbPlatform Oracle10g = new DbPlatform(Platform.Oracle, 10);

        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "g")]
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "DbPlatform is immutable.")]
        public static readonly DbPlatform Oracle11g = new DbPlatform(Platform.Oracle, 11);

        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "c")]
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "DbPlatform is immutable.")]
        public static readonly DbPlatform Oracle12c = new DbPlatform(Platform.Oracle, 12);

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "DbPlatform is immutable.")]
        public static readonly DbPlatform SQLite3 = new DbPlatform(Platform.SQLite, 3);

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "DbPlatform is immutable.")]
        public static readonly DbPlatform SqlServer2005 = new DbPlatform(Platform.SqlServer, 9);

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "DbPlatform is immutable.")]
        public static readonly DbPlatform SqlServer2008 = new DbPlatform(Platform.SqlServer, 10);

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "DbPlatform is immutable.")]
        public static readonly DbPlatform SqlServer2012 = new DbPlatform(Platform.SqlServer, 11);

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "DbPlatform is immutable.")]
        public static readonly DbPlatform SqlServer2014 = new DbPlatform(Platform.SqlServer, 12);

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "DbPlatform is immutable.")]
        public static readonly DbPlatform Teradata12 = new DbPlatform(Platform.Teradata, 12);

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "DbPlatform is immutable.")]
        public static readonly DbPlatform Teradata13 = new DbPlatform(Platform.Teradata, 13);

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "DbPlatform is immutable.")]
        public static readonly DbPlatform Teradata14 = new DbPlatform(Platform.Teradata, 14);

// ReSharper restore InconsistentNaming
#pragma warning restore 1591


        internal const int MaximumMajorVersion = 1000;

        /// <summary>
        /// Gets the database platform.
        /// </summary>
        public Platform Platform { get; private set; }

        /// <summary>
        /// Gets the major version of the database platform.
        /// </summary>
        public int MajorVersion { get; private set; }

        /// <summary>
        /// Gets the <see cref="Driver"/> by which the database is accessed.
        /// </summary>
        public Driver Driver { get; private set; }

        /// <summary>
        /// Initializes a new database platform specifier.
        /// </summary>
        public DbPlatform(Platform platform, int majorVersion, Driver driver)
        {
            if (majorVersion <= 0) throw new ArgumentOutOfRangeException("majorVersion");

            Platform = platform;
            MajorVersion = majorVersion;
            Driver = driver;
        }

        /// <summary>
        /// Initializes a new ADO.NET database platform specifier.
        /// </summary>
        public DbPlatform(Platform platform, int majorVersion)
            : this(platform, majorVersion, Driver.AdoNet)
        {
        }

        /// <summary>
        /// Returns a textual representation.
        /// </summary>
        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "{0}, Version: {1}, Driver: {2}", Platform, MajorVersion != 0 ? (object)MajorVersion : "Any", Driver);
        }

        internal bool Matches(DbPlatform dbPlatform)
        {
            return Platform == dbPlatform.Platform && MajorVersion == dbPlatform.MajorVersion && Driver == dbPlatform.Driver;
        }
    }
}