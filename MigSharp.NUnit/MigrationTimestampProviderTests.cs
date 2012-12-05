using System;
using System.CodeDom.Compiler;
using System.Globalization;
using System.Linq;
using System.Reflection;
using MigSharp.Process;
using NUnit.Framework;
using Rhino.Mocks;

namespace MigSharp.NUnit
{
    [TestFixture, Category("smoke")]
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
            var timestamp = provider.GetTimestamp(typeof (TimestampAttributeTestMigration));

            Assert.AreEqual(timestamp, 201211171825);
        }

        private class AttributeMigrationTimestampProvider : IMigrationTimestampProvider
        {
            public long GetTimestamp(Type migration)
            {
                if (migration == null) throw new ArgumentNullException("migration");

                var timestampAttr = (MigrationTimestampAttribute)migration.GetCustomAttributes(typeof (MigrationTimestampAttribute), false).FirstOrDefault();

                Assert.IsNotNull(timestampAttr, string.Format(CultureInfo.CurrentCulture, "Could find timestamp attribute on migration ({0}). Types implementing migrations using the AttributeMigrationTimestampProvider must have a MigrationTimestamp attribute.", migration.Name));
                return timestampAttr.Timestamp;
            }
        }

        [MigrationTimestamp(201211171825)]
        private class TimestampAttributeTestMigration
        {
        }

        private sealed class MigrationTimestampAttribute : Attribute
        {
            public long Timestamp { get; private set; }

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
            var timestamp = provider.GetTimestamp(typeof (TimestampInterfaceTestMigration));

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
                Assert.IsNotNull(instance, string.Format(CultureInfo.CurrentCulture, "Could find timestamp interface on migration ({0}). Types implementing migrations using the InterfaceMigrationTimestampProvider must implement the IMigrationTimestamp interface.", migration.Name));

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
            var migrator = new Migrator("not-used", ProviderNames.SqlServer2005);
            var versioning = MockRepository.GenerateStub<IVersioning>();
            versioning.Expect(v => v.ExecutedMigrations).Return(Enumerable.Empty<IMigrationMetadata>()); // pretend, no migrations ran so far
            migrator.UseCustomVersioning(versioning);

            IMigrationBatch batch = migrator.FetchMigrations(_timestampModuleTestAssembly);
            
            Assert.AreEqual(4, batch.ScheduledMigrations.Count);

            IScheduledMigrationMetadata migration = batch.ScheduledMigrations.Single(m => m.ModuleName == MigrationExportAttribute.DefaultModuleName);
            Assert.AreEqual(1, migration.Timestamp);

            migration = batch.ScheduledMigrations.Single(m => m.ModuleName == "NonDefaultModuleTreatedWithDefaultTimestampProvider");
            Assert.AreEqual(23, migration.Timestamp);

            migration = batch.ScheduledMigrations.Single(m => m.ModuleName == "ModuleA");
            Assert.AreEqual(201110251455L, migration.Timestamp);

            migration = batch.ScheduledMigrations.Single(m => m.ModuleName == "ModuleB");
            Assert.AreEqual(201211171825L, migration.Timestamp);
        }

        [Test, ExpectedException(typeof(ArgumentException), ExpectedMessage = "Cannot have more than one timestamp provider responsible for module: 'TimestampProviderDuplicateTest'.")]
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
                .Where(a => !AssemblyIsDynamic(a)) // note: assembly.IsDynamic only works for .NET 4.0 and higher
                .Select(a => a.Location);
            parameters.ReferencedAssemblies.AddRange(assemblies.ToArray());

            _timestampModuleTestAssembly = Compile(parameters, TimestampModuleTestAssemblySource);
            _duplicateProviderTestAssembly = Compile(parameters, DuplicateTimestampProviderAssemblySource);
        }

        private static bool AssemblyIsDynamic(Assembly assembly)
        {
            return assembly.ManifestModule.Name == "<In Memory Module>";
        }

        private static Assembly Compile(CompilerParameters parameters, string sources)
        {
            CompilerResults result = CodeDomProvider
                .CreateProvider("C#")
                .CompileAssemblyFromSource(parameters, sources);
            if (result.Errors.Count != 0)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Error compiling: {0}", sources));
            }
            return result.CompiledAssembly;
        }

        // Code to test module-specifc timestamp providers
        private const string TimestampModuleTestAssemblySource = @"
            using System;
            using System.Reflection;
            using System.Text.RegularExpressions;
            using MigSharp;
                
            [MigrationExport]
            public class Migration1 : IMigration
            {
               public void Up(IDatabase db)
               {
                   throw new NotSupportedException();
               }
            };
            
            [MigrationExport(ModuleName = ""NonDefaultModuleTreatedWithDefaultTimestampProvider"")]
            public class Migration23 : IMigration
            {
               public void Up(IDatabase db)
               {
                   throw new NotSupportedException();
               }
            };

            namespace ModuleA 
            {
                [MigrationTimestampProviderExport(ModuleName = ""ModuleA"")]
                public class TestMigrationTimestampProvider : IMigrationTimestampProvider
                {
                    public long GetTimestamp(Type migration)
                    {
                        Match match = Regex.Match(migration.Name, @""^M_([\d_]+)_\D.+"");
                        if (!match.Success) throw new InvalidOperationException(""The ModuleA migrations must have this naming pattern."");
                        return long.Parse(match.Groups[1].Value.Replace(""_"", """"));
                    }
                }

                [MigrationExport(ModuleName = ""ModuleA"")]
                public class M_2011_10_25_1455_SomeNameHere : IMigration
                {
                   public void Up(IDatabase db)
                   {
                       throw new NotSupportedException();
                   }
                };
            }

            namespace ModuleB
            {
                [MigrationTimestampProviderExport(ModuleName = ""ModuleB"")]
                public class AttributeMigrationTimestampProvider : IMigrationTimestampProvider
                {
                    public long GetTimestamp(Type migration)
                    {
                        var timestampAttr = (MigrationTimestampAttribute)migration.GetCustomAttributes(typeof(MigrationTimestampAttribute), false)[0];
                        return timestampAttr.Timestamp;
                    }
                };

                internal sealed class MigrationTimestampAttribute : Attribute
                {
                    public long Timestamp { get; private set; }

                    public MigrationTimestampAttribute(long timestamp)
                    {
                        Timestamp = timestamp;
                    }
                };
            
                [MigrationExport(ModuleName = ""ModuleB"")]
                [MigrationTimestamp(201211171825)]
                public class AddCustomerTableMigration : IMigration
                {
                   public void Up(IDatabase db)
                   {
                       throw new NotSupportedException();
                   }
                };
            }
";

        // Code to test duplicate timestamp provider error handling
        private const string DuplicateTimestampProviderAssemblySource =
            @"
            using System;
            using MigSharp;
            [MigrationTimestampProviderExport(ModuleName = ""TimestampProviderDuplicateTest"")]
            public class TestMigrationTimestampProvider1 : IMigrationTimestampProvider
            {
               public long GetTimestamp(Type migration)
               {
                   throw new NotSupportedException();
               }
            }
            
            [MigrationTimestampProviderExport(ModuleName = ""TimestampProviderDuplicateTest"")]
            public class TestMigrationTimestampProvider2 : IMigrationTimestampProvider
            {
               public long GetTimestamp(Type migration)
               {
                   throw new NotSupportedException();
               }
            }
            
            [MigrationExport(ModuleName = ""TimestampProviderDuplicateTest"")]
            public class TestTimestampDuplicateMigration : IMigration
            {
               public void Up(IDatabase db)
               {
                   throw new NotSupportedException();
               }
            };";

        #endregion
    }
}