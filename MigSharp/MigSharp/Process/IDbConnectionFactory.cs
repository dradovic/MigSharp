using System.Data;
using System.Data.Common;

namespace MigSharp.Process
{
    internal interface IDbConnectionFactory
    {
        IDbConnection OpenConnection(ConnectionInfo connectionInfo);
        DbProviderFactory GetDbProviderFactory(ConnectionInfo connectionInfo);
    }
}