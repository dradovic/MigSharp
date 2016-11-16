using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.SqlServer.Management.Smo;
using MigSharp.Generate;
using MigSharp.NUnit.Integration;
using NUnit.Framework;

namespace MigSharp.SqlServer.NUnit
{
    [TestFixture, Category("SqlServer2012")]
    public class SqlServer2012IntegrationTests : SqlServerIntegrationTests
    {
        private const string TestDbName2 = TestDbName + "_2";

        private Database _database2;

        private static string ConnectionString2
        {
            get
            {
                var builder = new SqlConnectionStringBuilder
                {
                    DataSource = Server,
                    InitialCatalog = TestDbName2,
                    IntegratedSecurity = true,
                };
                return builder.ConnectionString;
            }
        }

        protected override DbPlatform DbPlatform { get { return DbPlatform.SqlServer2012; } }

        [Test]
        public void TestGenerate() // CLEAN: add a test that tests FirstMigration mode (just drop MigSharp table)
        {
            SetupGenerateTest();

            // run all migration on second database
            Migrator migrator2 = new Migrator(ConnectionString2, DbPlatform);
            migrator2.MigrateAll(typeof(IntegrationTestsBase).Assembly);

            // generate base-line migration
            var options = new GeneratorOptions
            {
                Namespace = GetType().Namespace,
                IncludedSchemas = { "dbo", "Schema25", "Schema 29" }, // these schemas belong to the Default module
                ExcludedTables = { "Order Space" }, // the excluded tables belong other modules
                IncludeData = true,
            };
            SqlMigrationGeneratorFactory factory = new SqlMigrationGeneratorFactory(ConnectionString2);
            IGenerator generator = factory.Create(options);
            string migration = generator.Generate();
            Assert.IsEmpty(generator.Errors);
            Assert.IsTrue(migration.Contains("[AggregateMigrationExport"), "The generated migration should be an aggregated migration.");

            // compile and execute base-line migration on usual test database
            Assembly assembly = Compile(migration);
            Migrator migrator = new Migrator(ConnectionString, DbPlatform);
            migrator.MigrateAll(assembly, typeof(IntegrationTestsBase).Assembly);

            // check if the schema of the two databases is equal
            CollectionAssert.AreEqual(_database2.Tables.Cast<Table>().Select(t => t.Name).ToArray(), DatabaseSmo.Tables.Cast<Table>().Select(t => t.Name).ToArray(), "The tables differ.");
            IEnumerable<string> script = ScriptDatabase(DatabaseSmo);
            IEnumerable<string> script2 = ScriptDatabase(_database2);
            CollectionAssert.AreEqual(script2.ToArray(), script.ToArray());

            // check if the data was scripted too
            VerifyResultsOfAllMigrations();
        }

        private IEnumerable<string> ScriptDatabase(Database database)
        {
            ScriptingOptions options = new ScriptingOptions();
            options.DriAll = true;
            options.ClusteredIndexes = true;
            options.Default = true;
            options.Indexes = true;
            options.IncludeHeaders = true;
            options.DriDefaults = true;
            options.IncludeHeaders = false;
            //options.FileName = Path.GetTempFileName();
            //Console.WriteLine("Scripted database to: {0}", options.FileName);
            Scripter scripter = new Scripter(ServerSmo) { Options = options };
            return scripter.EnumScript(database.Tables.OfType<Table>().ToArray())
                .Concat(scripter.EnumScript(database.UserDefinedTableTypes.OfType<UserDefinedTableType>().ToArray()));
        }

        private static Assembly Compile(string migration)
        {
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(migration);
            string assemblyName = Path.GetRandomFileName();
            MetadataReference[] references = new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location), // System
                MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location), // System.Linq
                MetadataReference.CreateFromFile(typeof(GeneratedCodeAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(DbType).Assembly.Location), // System.Data
                MetadataReference.CreateFromFile(typeof(ExportAttribute).Assembly.Location), // System.ComponentModel.Composition
                MetadataReference.CreateFromFile(typeof(IDatabase).Assembly.Location), // MigSharp
            };
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
                CollectionAssert.IsEmpty(compilationErrors, "Could not compile migration."); // this nicely outputs the compilation errors at the top of the output
                Assert.IsTrue(result.Success, "Could not compile migration.");
                ms.Seek(0, SeekOrigin.Begin);
                return Assembly.Load(ms.ToArray());
            }
        }

        public void SetupGenerateTest()
        {
            var database = ServerSmo.Databases[TestDbName2];
            if (database != null)
            {
                database.Drop();
            }

            _database2 = new Database(ServerSmo, TestDbName2);
            _database2.Create();
        }
    }
}