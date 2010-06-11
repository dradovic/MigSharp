using System.Data;

namespace MigSharp.Process
{
    internal interface IDbConnectionFactory
    {
        IDbConnection OpenConnection(ConnectionInfo connectionInfo);
    }
}