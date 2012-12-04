using System;
using System.CodeDom.Compiler;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
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

        #region Migrator uses module-specifc timestamp provider

        [Test]
        public void MigratorUsesModuleSpecificTimestampProvider()
        {
            var errorThrown = false;
            var migrator = new Migrator("not-used", ProviderNames.SQLite, new MigrationOptions("TimestampProviderTest"));
            try
            {
                migrator.MigrateTo(_timestampModuleTestAssembly, 1);
            }
            catch (NotImplementedException ex)
            {
                Assert.AreEqual("TimestampProviderTest called", ex.Message);
                errorThrown = true;
            }

            Assert.IsTrue(errorThrown, "Timestamp Provider not called");
        }

        [Test, ExpectedException(typeof(InvalidOperationException), ExpectedMessage = "Cannot have more than one exported timestamp provider with the same module name.")]
        public void MigratorThrowsErrorIfDuplicateTimestampProvidersFoundForModule()
        {
            var migrator = new Migrator("not-used", ProviderNames.SQLite, new MigrationOptions("TimestampProviderDuplicateTest"));
            migrator.MigrateTo(_duplicateProviderTestAssembly, 1);
        }

        
        /* The following code dynamically compiles test assemblies into memory. 
         * This is done to stop intentionally-broken timestamp provider code interfering with integration tests
         * and removes the need for additional assemblies to be created.
         */

        private Assembly _timestampModuleTestAssembly;
        private Assembly _duplicateProviderTestAssembly;

        [TestFixtureSetUp]
        public void SetupTimestampProvider()
        {
            // Configure the compiler to generate in-memory
            var parameters = new CompilerParameters
                                 {
                                     GenerateExecutable = false,
                                     GenerateInMemory = true
                                 };

            // Add assemblies referenced by this assembly be referenced by the compiled assembly
            var assemblies = AppDomain.CurrentDomain
                .GetAssemblies()
                .Where(a => !a.IsDynamic)
                .Select(a => a.Location);
            parameters.ReferencedAssemblies.AddRange(assemblies.ToArray());

            // Compile the time stamp module test code
            var result = CodeDomProvider.CreateProvider("C#")
                .CompileAssemblyFromSource(parameters, TimestampModuleTestAssemblySource.ToString());
            if (result.Errors.Count != 0)
                throw new InvalidOperationException("TimestampModuleTestAssemblySource generated errors when compiled. Please check.");
            _timestampModuleTestAssembly = result.CompiledAssembly;

            // Compile the duplicate time stamp provider test code
            result = CodeDomProvider.CreateProvider("C#")
                .CompileAssemblyFromSource(parameters, DuplicateTimestampProviderAssemblySource.ToString());
            if (result.Errors.Count != 0)
                throw new InvalidOperationException("DuplicateTimestampProviderAssemblySource generated errors when compiled. Please check.");
            _duplicateProviderTestAssembly = result.CompiledAssembly;
        }

        // Code to test module-specifc timestamp providers
        private static readonly StringBuilder TimestampModuleTestAssemblySource = new StringBuilder()
            .AppendLine("using System;")
            .AppendLine("using MigSharp;")
            .AppendLine("[MigrationTimestampProviderExport(ModuleName = \"TimestampProviderTest\")]")
            .AppendLine("public class TestMigrationTimestampProvider : IMigrationTimestampProvider")
            .AppendLine("{")
            .AppendLine("   public long GetTimestamp(Type migration)")
            .AppendLine("   {")
            .AppendLine("       throw new NotImplementedException(\"TimestampProviderTest called\");")
            .AppendLine("   }")
            .AppendLine("}")
            .AppendLine()
            .AppendLine("[MigrationExport(ModuleName = \"TimestampProviderTest\")]")
            .AppendLine("public class TestTimestampMigration : IMigration")
            .AppendLine("{")
            .AppendLine("   public void Up(IDatabase db)")
            .AppendLine("   {")
            .AppendLine("       throw new NotImplementedException();")
            .AppendLine("   }")
            .AppendLine("}");

        // Code to test duplicate timestamp provider error handling
        private static readonly StringBuilder DuplicateTimestampProviderAssemblySource = new StringBuilder()
            .AppendLine("using System;")
            .AppendLine("using MigSharp;")
            .AppendLine("[MigrationTimestampProviderExport(ModuleName = \"TimestampProviderDuplicateTest\")]")
            .AppendLine("public class TestMigrationTimestampProvider1 : IMigrationTimestampProvider")
            .AppendLine("{")
            .AppendLine("   public long GetTimestamp(Type migration)")
            .AppendLine("   {")
            .AppendLine("       throw new NotImplementedException();")
            .AppendLine("   }")
            .AppendLine("}")
            .AppendLine()
            .AppendLine("[MigrationTimestampProviderExport(ModuleName = \"TimestampProviderDuplicateTest\")]")
            .AppendLine("public class TestMigrationTimestampProvider2 : IMigrationTimestampProvider")
            .AppendLine("{")
            .AppendLine("   public long GetTimestamp(Type migration)")
            .AppendLine("   {")
            .AppendLine("       throw new NotImplementedException();")
            .AppendLine("   }")
            .AppendLine("}")
            .AppendLine()
            .AppendLine("[MigrationExport(ModuleName = \"TimestampProviderDuplicateTest\")]")
            .AppendLine("public class TestTimestampDuplicateMigration : IMigration")
            .AppendLine("{")
            .AppendLine("   public void Up(IDatabase db)")
            .AppendLine("   {")
            .AppendLine("       throw new NotImplementedException();")
            .AppendLine("   }")
            .AppendLine("}");

        #endregion
    }
}
