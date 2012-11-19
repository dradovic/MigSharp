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
        public void MigratorUsesModuleSpecificTimestampProvider()
        {
            var errorThrown = false;
            var migrator = new Migrator("not-used", ProviderNames.SQLite, new MigrationOptions("TimestampProviderTest"));
            try
            {
                migrator.MigrateTo(typeof (Migration1).Assembly, 1);
            }
            catch (NotImplementedException ex)
            {
                Assert.AreEqual("TimestampProviderTest called", ex.Message);
                errorThrown = true;
            }

            Assert.IsTrue(errorThrown, "Timestamp Provider not called");
        }

        [Test, ExpectedException(typeof(InvalidOperationException), ExpectedMessage = "There is more than one timestamp provider for the module 'TimestampProviderDuplicateTest'. Cannot have more than one timestamp provider exported for a module in an assembly.")]
        public void MigratorThrowsErrorIfDuplicateTimestampProvidersFoundForModule()
        {
            var migrator = new Migrator("not-used", ProviderNames.SQLite, new MigrationOptions("TimestampProviderDuplicateTest"));            
            migrator.MigrateTo(typeof (Migration1).Assembly, 1);
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

    #region Supporting classes for module-specific timestamp provider test

    [MigrationTimestampProviderExport(ModuleName = "TimestampProviderTest")]
    public class TestMigrationTimestampProvider : IMigrationTimestampProvider
    {
        public long GetTimestamp(Type migration)
        {
            throw new NotImplementedException("TimestampProviderTest called");
        }
    }

    [MigrationExport(ModuleName = "TimestampProviderTest")]
    public class TestTimestampMigration : IMigration
    {
        public void Up(IDatabase db)
        {
            throw new NotImplementedException();
        }
    }

    [MigrationTimestampProviderExport(ModuleName = "TimestampProviderDuplicateTest")]
    public class TestMigrationTimestampProvider1 : IMigrationTimestampProvider
    {
        public long GetTimestamp(Type migration)
        {
            throw new NotImplementedException();
        }
    }

    [MigrationTimestampProviderExport(ModuleName = "TimestampProviderDuplicateTest")]
    public class TestMigrationTimestampProvider2 : IMigrationTimestampProvider
    {
        public long GetTimestamp(Type migration)
        {
            throw new NotImplementedException();
        }
    }

    [MigrationExport(ModuleName = "TimestampProviderDuplicateTest")]
    public class TestTimestampDuplicateMigration : IMigration
    {
        public void Up(IDatabase db)
        {
            throw new NotImplementedException();
        }
    }

    #endregion
}
