using System;
using System.Globalization;
using System.Linq;
using MigSharp.NUnit.Integration;
using NUnit.Framework;

namespace MigSharp.NUnit
{
    [TestFixture]
    public class MigrationTimestampProviderTests
    {
        #region Default timestamp provider

        [Test]
        public void TestDefaultProvider()
        {
            var provider = new DefaultMigrationTimestampProvider();
            var timestamp = provider.GetTimestamp(typeof (TimestampTestMigration201211171806));

            Assert.AreEqual(timestamp, 201211171806);
        }

        private class TimestampTestMigration201211171806
        {
        }

        #endregion

        #region Migrator uses timestamp provider provided by MigrationOptions

        [Test]
        public void MigratorUsesTimestampProviderSpecifiedInOptions()
        {
            // Get the total number of migrations
            var totalMigrations = typeof (Migration1).Assembly.GetTypes()
                .Count(t => !t.IsAbstract && t.IsClass && t.GetInterfaces().Any(i => i == typeof (IMigration)));

            var mockProvider = new MockMigrationTimestampProvider();
            var migrator = new Migrator("not-used", ProviderNames.SQLite, new MigrationOptions
                                                                              {
                                                                                  TimestampProvider = mockProvider
                                                                              });
            try
            {
                migrator.MigrateTo(typeof (Migration1).Assembly, 1);
            }
            catch (Exception)
            {
                // We know it's going to fail as the connection string is garbage
            }

            Assert.AreEqual(totalMigrations, mockProvider.TimesCalled);
        }

        private class MockMigrationTimestampProvider : IMigrationTimestampProvider
        {
            public int TimesCalled { get; private set; }

            public long GetTimestamp(Type migration)
            {
                return TimesCalled++;
            }
        }

        #endregion

        #region Example attribute timestamp provider

        [Test]
        public void TestExampleAttributeProvider()
        {
            var provider = new AttributeMigrationTimestampProvider();
            var timestamp = provider.GetTimestamp(typeof(TimestampAttributeTestMigration));

            Assert.AreEqual(timestamp, 201211171825);
        }

        private class AttributeMigrationTimestampProvider : IMigrationTimestampProvider
        {
            public long GetTimestamp(Type migration)
            {
                if (migration == null) throw new ArgumentNullException("migration");

                var timestampAttr = (MigrationTimestampAttribute) migration.GetCustomAttributes(typeof (MigrationTimestampAttribute), false).FirstOrDefault();

                if (timestampAttr == null)
                    throw new ArgumentException(string.Format(CultureInfo.CurrentCulture,
                                                              "Could find timestamp attribute on migration ({0}). Types implementing migrations using the AttributeMigrationTimestampProvider must have a MigrationTimestamp attribute.",
                                                              migration.Name));

                return timestampAttr.Timestamp;
            }
        }

        [MigrationTimestamp(201211171825)]
        private class TimestampAttributeTestMigration
        {
        }

        private class MigrationTimestampAttribute : Attribute
        {
            public long Timestamp { get; set; }

            public MigrationTimestampAttribute(long timestamp)
            {
                Timestamp = timestamp;
            }
        }

        #endregion

        #region Example interface timestamp provider

        [Test]
        public void TestExampleInterfaceProvider()
        {
            var provider = new InterfaceMigrationTimestampProvider();
            var timestamp = provider.GetTimestamp(typeof(TimestampInterfaceTestMigration));

            Assert.AreEqual(timestamp, 201211171833);
        }

        private class InterfaceMigrationTimestampProvider : IMigrationTimestampProvider
        {
            public long GetTimestamp(Type migration)
            {
                if (migration == null) throw new ArgumentNullException("migration");

                IMigrationTimestamp instance;
                try
                {
                    instance = Activator.CreateInstance(migration) as IMigrationTimestamp;
                }
                catch (Exception ex)
                {
                    throw new ArgumentException(string.Format(CultureInfo.CurrentCulture,
                                                              "Could not create an instance of migration {0}. Please make sure the migration has a parameterless constructor.",
                                                              migration.Name), ex);
                }

                if (instance == null)
                    throw new ArgumentException(string.Format(CultureInfo.CurrentCulture,
                                                              "Could find timestamp interface on migration ({0}). Types implementing migrations using the InterfaceMigrationTimestampProvider must implement the IMigrationTimestamp interface.",
                                                              migration.Name));

                return instance.Timestamp;
            }
        }

        private class TimestampInterfaceTestMigration : IMigrationTimestamp
        {
            public long Timestamp
            {
                get { return 201211171833; }
            }
        }

        private interface IMigrationTimestamp
        {
            long Timestamp { get; }
        }

        #endregion
    }
}
