using System;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Reflection;

using MigSharp.NUnit.Integration;

using NUnit.Framework;

namespace MigSharp.SqlServerCe.NUnit
{
    public abstract class SqlServerCeIntegrationTestsBase : IntegrationTestsBase
    {
        private string _dataFile;
        private Assembly _ceAssembly;

        protected abstract string CeVersion { get; }

        protected override string ConnectionString { get { return "Data Source=" + _dataFile; } }

        private static void CopyFolder(string sourceFolder, string destFolder, bool overwrite)
        {
            if (!Directory.Exists(destFolder))
            {
                Directory.CreateDirectory(destFolder);
            }
            string[] files = Directory.GetFiles(sourceFolder);
            foreach (string file in files)
            {
                string name = Path.GetFileName(file);
                if (name != null)
                {
                    string dest = Path.Combine(destFolder, name);
                    File.Copy(file, dest, overwrite);
                }
            }
            string[] folders = Directory.GetDirectories(sourceFolder);
            foreach (string folder in folders)
            {
                string name = Path.GetFileName(folder);
                if (name != null)
                {
                    string dest = Path.Combine(destFolder, name);
                    CopyFolder(folder, dest, overwrite);
                }
            }
        }

        [TestFixtureSetUp]
        public void FixtureSetup()
        {
            // this is only needed because when started with the TeamCity test runner,
            // the private probing path is ignored for some reason
            CopyFolder(Environment.CurrentDirectory + "\\Ce" + CeVersion, Environment.CurrentDirectory, true);

            _ceAssembly = Assembly.LoadFrom(@".\System.Data.SqlServerCe.dll");
        }

        public override void Setup()
        {
            base.Setup();

            _dataFile = Path.GetTempFileName();
            File.Delete(_dataFile);

            using (dynamic engine = CreateInstance("SqlCeEngine", ConnectionString))
            {
                engine.CreateDatabase();
            }
        }

        protected override DbDataAdapter GetDataAdapter(string tableName, out DbCommandBuilder builder)
        {
            DbDataAdapter adapter = (DbDataAdapter)CreateInstance("SqlCeDataAdapter", string.Format(CultureInfo.InvariantCulture, "SELECT * FROM \"{0}\"", tableName), ConnectionString);
            builder = (DbCommandBuilder)CreateInstance("SqlCeCommandBuilder", adapter);
            return adapter;
        }

        private object CreateInstance(string typeName, params object[] args)
        {
            Type type = _ceAssembly.GetType("System.Data.SqlServerCe." + typeName);
            return Activator.CreateInstance(type, args);
        }

        public override void Teardown()
        {
            File.Delete(_dataFile);

            base.Teardown();
        }

        [Test]
        public void VerifyCorrectCeVersionIsLoaded()
        {
            // note: only one CE version can be tested in one run
            // (otherwise this test with raise an alarm)
            Assert.IsTrue(_ceAssembly.FullName.Contains("Version=" + CeVersion), 
                "Expected Ce " + CeVersion + " but using " + _ceAssembly.FullName);
        }

        [Test]
        public override void TestMigration1UsingConsoleApp()
        {
            // we don't execute this test yet since the Migrate.exe
            // would require a config file that includes the definition of
            // the .Net Framework Data Provider
        }
    }
}