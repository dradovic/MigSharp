using System;
using System.Data.Odbc;
using System.Threading;

namespace MigSharp.NUnit.Integration
{
    public static class OdbcIntegrationTestHelper
    {
        public static void CloseAllOdbcConnections()
        {
            try
            {
                OdbcConnection.ReleaseObjectPool();

                // clean up any connections that have not been disposed yet
                GC.Collect(); 
                GC.WaitForPendingFinalizers();

                Thread.Sleep(1000); // give databse a chance to release connections
            }
            catch (Exception ex)
            {
                Console.WriteLine("Can't clear ODBC connection pools: " + ex.Message);
                throw;
            }
        }
    }
}