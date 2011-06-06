using System.Data;

namespace MigSharp.Process
{
    internal class ExecutingCommandExecutor : ScriptingCommandExecutor
    {
        public ExecutingCommandExecutor(ISqlScripter sqlScripter) : base(sqlScripter)
        {
        }

        public override void ExecuteNonQuery(IDbCommand command)
        {
            base.ExecuteNonQuery(command);

            command.ExecuteNonQuery();
        }
    }
}