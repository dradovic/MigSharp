using System.Data;

using MigSharp.Core;

namespace MigSharp.Process
{
    internal class LoggingScripter : ISqlScripter
    {
        public void Script(IDbCommand command)
        {
            Log.Verbose(LogCategory.Sql, command.CommandText);
        }
    }
}