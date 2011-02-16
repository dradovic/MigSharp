namespace MigSharp.Core.Commands
{
    internal class AlterColumnCommand : Command
    {
        private readonly string _columnName;

        public string ColumnName { get { return _columnName; } }
        public new AlterTableCommand Parent { get { return (AlterTableCommand)base.Parent; } }

        public AlterColumnCommand(AlterTableCommand parent, string columnName) : base(parent)
        {
            _columnName = columnName;
        }
    }
}