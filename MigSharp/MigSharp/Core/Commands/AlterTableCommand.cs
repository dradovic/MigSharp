namespace MigSharp.Core.Commands
{
    internal class AlterTableCommand : Command
    {
        private readonly string _tableName;

        public string TableName { get { return _tableName; } }

        public AlterTableCommand(string tableName)
        {
            _tableName = tableName;
        }
    }
}