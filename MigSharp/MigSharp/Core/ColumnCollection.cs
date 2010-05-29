using MigSharp.Core.Commands;

namespace MigSharp.Core
{
    internal class ColumnCollection : AdHocCollection<Column>, IColumnCollection
    {
        private readonly AlterTableCommand _alterTableCommand;

        public ColumnCollection(AlterTableCommand alterTableCommand)
        {
            _alterTableCommand = alterTableCommand;
        }

        protected override Column CreateItem(string name)
        {
            AlterColumnCommand alterColumnCommand = new AlterColumnCommand(name);
            _alterTableCommand.Add(alterColumnCommand);
            return new Column(alterColumnCommand);
        }
    }
}