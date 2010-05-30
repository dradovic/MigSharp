using System.Collections.Generic;
using System.Data;

using MigSharp.Core;
using MigSharp.Providers;
using MigSharp.Smo;

using NUnit.Framework;

namespace MigSharp.NUnit.Provider
{
    [TestFixture]
    public class SqlServerProviderVsSmoTests
    {
        [Test, Explicit]
        public void TestCreateTable()
        {
            var db = new Database();
            db.CreateTable("Customers")
                .WithPrimaryKeyColumn("ObjectKey", DbType.Int32)
                .WithPrimaryKeyColumn("AnalysisKey", DbType.Int32)
                .WithNullableColumn("Name", DbType.String);
            IProvider sqlProvider = new SqlServerProvider();
            IProvider smoProvider = new SmoProvider();
            AssertAreEqual(sqlProvider, smoProvider, db);

            //Server server = new Server();
            //Database database = new Database(server, "MyDatabase");
            //Table table = new Table(database, "MyTable");

            ////table.Rename("neasdfaw");
            //MethodInfo scriptRename = table.GetType().GetMethod("ScriptRename", BindingFlags.Instance | BindingFlags.NonPublic);
            //Assert.IsNotNull(scriptRename);
            //StringCollection query = new StringCollection();
            //ScriptingOptions options = new ScriptingOptions();
            //scriptRename.Invoke(table, new object[] { query, options, "NewName" });
            //foreach (string s in query)
            //{
            //    Trace.WriteLine(s);
            //}

            //Column column = new Column(table, "MyTable")
            //{
            //    DataType = DataType.BigInt
            //};
            //table.Columns.Add(column);
            //foreach (string s in table.Script())
            //{
            //    Trace.WriteLine(s);
            //}
        }

        private static void AssertAreEqual(IProvider sqlProvider, IProvider smoProvider, Database database)
        {
            CommandScripter sqlScripter = new CommandScripter(sqlProvider);
            CommandScripter smoScripter = new CommandScripter(smoProvider);
            CollectionAssert.AreEqual(
                new List<string>(smoScripter.GetCommandTexts(database)),
                new List<string>(sqlScripter.GetCommandTexts(database)));
        }
    }
}