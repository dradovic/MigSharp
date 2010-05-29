namespace MigSharp.Core.Commands
{
    internal class AlterColumnCommand : Command
    {
        private readonly string _columnName;

        public string ColumnName { get { return _columnName; } }

        public AlterColumnCommand(string columnName)
        {
            _columnName = columnName;
        }
    }
}