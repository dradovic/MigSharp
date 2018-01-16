using System;
using System.Collections.Generic;
using System.Composition;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using FakeItEasy;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using MigSharp.Process;
using NUnit.Framework;

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
            var timestamp = provider.GetTimestamp(typeof(TimestampTestMigration201211171806));

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

                var timestampAttr = (MigrationTimestampAttribute)migration.GetCustomAttributes(typeof(MigrationTimestampAttribute), false).FirstOrDefault();

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
                catch (Exception x)
                {
                    throw new ArgumentException(string.Format(CultureInfo.CurrentCulture,
                                                              "Could not create an instance of migration {0}. Please make sure the migration has a parameterless constructor.",
                                                              migration.Name), x);
                }
                Assert.IsNotNull(instance, string.Format(CultureInfo.CurrentCulture, "Could find timestamp interface on migration ({0}). Types implementing migrations using the InterfaceMigrationTimestampProvider must implement the IMigrationTimestamp interface.", migration.Name));

                return instance.Timestamp;
            }
        }

        private class TimestampInterfaceTestMigration : IMigrationTimestamp
        {
            public long Timestamp { get { return 201211171833; } }
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
            var migrator = new Migrator("not-used", DbPlatform.SqlServer2012);
            var versioning = A.Fake<IVersioning>();
            A.CallTo(() => versioning.ExecutedMigrations).Returns(Enumerable.Empty<IMigrationMetadata>()); // pretend no migrations ran so far
            migrator.UseCustomVersioning(versioning);

            IMigrationBatch batch = migrator.FetchMigrations(_timestampModuleTestAssembly);

            Assert.AreEqual(4, batch.Steps.Count);

            IMigrationStepMetadata step = batch.Steps.Single(m => m.ModuleName == MigrationExportAttribute.DefaultModuleName);
            Assert.AreEqual(1, step.Migrations.Single().Timestamp);

            step = batch.Steps.Single(m => m.ModuleName == "NonDefaultModuleTreatedWithDefaultTimestampProvider");
            Assert.AreEqual(23, step.Migrations.Single().Timestamp);

            step = batch.Steps.Single(m => m.ModuleName == "ModuleA");
            Assert.AreEqual(201110251455L, step.Migrations.Single().Timestamp);

            step = batch.Steps.Single(m => m.ModuleName == "ModuleB");
            Assert.AreEqual(201211171825L, step.Migrations.Single().Timestamp);
        }

        [Test]
        public void MigratorThrowsErrorIfDuplicateTimestampProvidersFoundForModule()
        {
            var migrator = new Migrator("not-used", DbPlatform.SQLite3, new MigrationOptions("TimestampProviderDuplicateTest"));
            Assert.That(() => migrator.MigrateTo(_duplicateProviderTestAssembly, 1), Throws.ArgumentException.With.Message.EqualTo("Cannot have more than one timestamp provider responsible for module: 'TimestampProviderDuplicateTest'."));
        }


        /* The following code dynamically compiles test assemblies into memory. 
         * This is done to stop intentionally-broken timestamp provider code interfering with integration tests
         * and removes the need for additional assemblies to be created.
         */

        private Assembly _timestampModuleTestAssembly;
        private Assembly _duplicateProviderTestAssembly;

        [OneTimeSetUpAttribute]
        public void SetupTimestampProvider()
        {
            _timestampModuleTestAssembly = Compile(TimestampModuleTestAssemblySource);
            _duplicateProviderTestAssembly = Compile(DuplicateTimestampProviderAssemblySource);
        }

        private static Assembly Compile(string sources)
        {
            string dotnetDir = Path.GetDirectoryName(typeof(object).Assembly.Location);
            Assert.IsNotNull(dotnetDir, "Cannot find base directory of the .NET runtime.");
            MetadataReference[] references =
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location), // System
                MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location), // System.Linq
                MetadataReference.CreateFromFile(typeof(Regex).Assembly.Location), // System.Text.RegularExpressions
                MetadataReference.CreateFromFile(typeof(DbType).Assembly.Location), // System.Data
                MetadataReference.CreateFromFile(typeof(ExportAttribute).Assembly.Location), // System.Composition
                MetadataReference.CreateFromFile(typeof(IDatabase).Assembly.Location), // MigSharp
                MetadataReference.CreateFromFile(Path.Combine(dotnetDir, "System.Runtime.dll")),
                MetadataReference.CreateFromFile(Assembly.Load("netstandard, Version=2.0.0.0").Location) // see: https://stackoverflow.com/questions/46421686/how-to-write-a-roslyn-analyzer-that-references-a-dotnet-standard-2-0-project
            };

            string assemblyName = Path.GetRandomFileName();
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(sources);
            CSharpCompilation compilation = CSharpCompilation.Create(assemblyName, new[] { syntaxTree }, references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            using (var ms = new MemoryStream())
            {
                EmitResult result = compilation.Emit(ms);

                List<string> compilationErrors = new List<string>();
                foreach (Diagnostic diagnostic in result.Diagnostics.Where(diagnostic =>
                    diagnostic.IsWarningAsError ||
                    diagnostic.Severity == DiagnosticSeverity.Error))
                {
                    compilationErrors.Add(diagnostic.Id + ": " + diagnostic.GetMessage());
                }
                CollectionAssert.IsEmpty(compilationErrors, "Could not compile sources."); // this nicely outputs the compilation errors at the top of the output
                Assert.IsTrue(result.Success, "Could not compile sources.");
                ms.Seek(0, SeekOrigin.Begin);
                return Assembly.Load(ms.ToArray());
            }
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