using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.Odbc;
using System.Globalization;
using MigSharp.NUnit.Integration;

using NUnit.Framework;

namespace MigSharp.Teradata.NUnit
{
    [TestFixture, Category("TeradataOdbc")]
    public class TeradataOdbcIntegrationTests : IntegrationTestsBase
    {
        protected static string Server
        {
            get
            {
                const string server = "MIGSHARP_TERADATA_SERVER";
                return GetEnvironmentVariable(server);
            }
        }

        protected static string Password
        {
            get
            {
                const string password = "MIGSHARP_TERADATA_PASSWORD";
                return GetEnvironmentVariable(password);
            }
        }

        protected static string User
        {
            get
            {
                const string user = "MIGSHARP_TERADATA_USER";
                return GetEnvironmentVariable(user);
            }
        }

        private readonly string _databaseName = GetUniqueDbName();
        protected string DatabaseName { get { return _databaseName; } }


        private static string GetUniqueDbName()
        {
            //Teradata only allows 30 chars for databse name and a guid is 38 so we use this shortend form
            //Oracle requires name to start with a letter no longer a true short guid as we use substring addedtimestamp to ensure unique and remove illegal chars
            return string.Format(CultureInfo.InvariantCulture, "A{0}{1}", Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Substring(0, 22).Replace("/", "_")
                .Replace("+", "_"), DateTime.Now.Minute.ToString(CultureInfo.CurrentCulture) + DateTime.Now.Second + DateTime.Now.Millisecond);
        }

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            RegisterProvider();

            try
            {
                DropDatabase(_databaseName);
            }
            catch (OdbcException)
            {
                Console.WriteLine("Just making sure database doesn't exist");
            }
            CreateDatabase(_databaseName, null);
        }

        //TODO : This does not quite work as expected need to re-vist 
        private static void RegisterProvider()
        {
            // Registering the private deployed Teradata provide using the SQL CE provider workaround as a base
            // http://stackoverflow.com/questions/1117683/add-a-dbproviderfactory-without-an-app-config


            DataTable dt = DbProviderFactories.GetFactoryClasses();
            foreach (DataRow row in dt.Rows)
            {
                Console.WriteLine(row[0].ToString());
            }
            
            DataSet dataSet = ConfigurationManager.GetSection("system.data") as DataSet;
            if (dataSet == null) throw new InvalidOperationException("cannot configure privately deployed Teradata native provider");

            DataRow[] dataRows = dataSet.Tables[0].Select(string.Format(CultureInfo.InvariantCulture, "InvariantName='{0}'", "Teradata.Client.Provider"));
            foreach (var dataRow in dataRows)
            {
                dataSet.Tables[0].Rows.Remove(dataRow);
            }

            dataSet.Tables[0].Rows.Add(
                ".NET Data Provider for Teradata",
                ".NET Framework Data Provider for Teradata",
                "Teradata.Client.Provider",
                "Teradata.Client.Provider.TdFactory, Teradata.Client.Provider, Version=13.1.0.4, Culture=neutral, PublicKeyToken=76b417ee2e04956c");
        }

        protected override DbDataAdapter GetDataAdapter(string tableName, out DbCommandBuilder builder)
        {
            var adapter = new OdbcDataAdapter(string.Format(CultureInfo.InvariantCulture, "SELECT * FROM \"{0}\" ORDER BY 1", tableName), OdbcConnectionString);
            builder = new OdbcCommandBuilder(adapter);
            return adapter;
        }

        protected string OdbcConnectionString { get { return string.Format(CultureInfo.InvariantCulture, "Driver={{Teradata}}; DBCName={0} ; Uid={1} ; Pwd={2} ; Database={3};USENATIVELOBSUPPORT=YES;", Server, User, Password, _databaseName); } }

        protected override string ConnectionString { get { return OdbcConnectionString; } }

        protected static string MasterConnectionString { get { return string.Format(CultureInfo.InvariantCulture, "Driver={{Teradata}}; DBCName={0} ; Uid={1} ; Pwd={2} ; Database={3};", Server, User, Password, "dbc"); } }

        protected override string ProviderName { get { return ProviderNames.TeradataOdbc; } }

        [TearDown]
        public override void Teardown()
        {
            base.Teardown();

            DropDatabase(_databaseName);
        }

        public static int CreateDatabase(string databaseName, Dictionary<string, string> options)
        {
            OdbcConnection con = new OdbcConnection(MasterConnectionString);
            con.Open();

            string permSpace = "10e6";
            if (options != null && options.ContainsKey("perm"))
                permSpace = options["perm"];

            OdbcCommand com = new OdbcCommand("CREATE DATABASE " + '"' + databaseName + '"' + " from dbc as PERM=" + permSpace, con);
            return com.ExecuteNonQuery();
        }

        public static int DropDatabase(string databaseName)
        {
            //Clean up any connections that have not been disposed yet
            GC.Collect();
            //give databse a chance to release connections
            GC.WaitForPendingFinalizers();

            var con = new OdbcConnection(MasterConnectionString);
            con.Open();

            var com = new OdbcCommand("DELETE DATABASE " + '"' + databaseName + '"', con);
            com.ExecuteNonQuery();
            com.CommandText = "DROP DATABASE " + '"' + databaseName + '"';
            return com.ExecuteNonQuery();
        }
    }
}