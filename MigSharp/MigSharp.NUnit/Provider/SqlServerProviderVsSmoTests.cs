using System.Collections.Specialized;
using System.Diagnostics;
using System.Reflection;

using Microsoft.SqlServer.Management.Smo;

using NUnit.Framework;

namespace MigSharp.NUnit.Provider
{
    [TestFixture]
    public class SqlServerProviderVsSmoTests
    {
        [Test, Explicit]
        public void TestCreateTable()
        {
            Server server = new Server();
            Database database = new Database(server, "MyDatabase");
            Table table = new Table(database, "MyTable");
           
            //table.Rename("neasdfaw");
            MethodInfo scriptRename = table.GetType().GetMethod("ScriptRename", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(scriptRename);
            StringCollection query = new StringCollection();
            ScriptingOptions options = new ScriptingOptions();
            scriptRename.Invoke(table, new object[] { query, options, "NewName" });
            foreach (string s in query)
            {
                Trace.WriteLine(s);
            }
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
    }
}