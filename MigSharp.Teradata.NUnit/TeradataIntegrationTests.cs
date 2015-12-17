using System;
using System.Configuration;
using System.Data;
using System.Globalization;

using NUnit.Framework;

namespace MigSharp.Teradata.NUnit
{
    [TestFixture, Category("Teradata")]
    public class TeradataIntegrationTests : TeradataOdbcIntegrationTests
    {
        protected override string ConnectionString
        {
            get
            {
                return string.Format(CultureInfo.InvariantCulture, "Database={0};User Id={1};Data Source='{2}';Password={3};Session Mode=ANSI",
                    DatabaseName, User, Server, Password);
            }
        }

        protected override DbPlatform DbPlatform { get { return DbPlatform.Teradata12; } }

        public override void Setup()
        {
            base.Setup();
            RegisterProvider();
        }

        private static void RegisterProvider()
        {
            // Registering the private deployed Teradata provide using the SQL CE provider workaround as a base
            // http://stackoverflow.com/questions/1117683/add-a-dbproviderfactory-without-an-app-config


            // Uncomment the follwing lines to see the existing factory classes:
            //DataTable dt = DbProviderFactories.GetFactoryClasses();
            //foreach (DataRow row in dt.Rows)
            //{
            //    Console.WriteLine(row[0].ToString());
            //}

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
    }
}