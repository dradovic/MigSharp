using MigSharp.Core.Commands;

namespace MigSharp.Core.Entities
{
    internal class IndexesCollection : AdHocCollection<IIndex>, IIndexesCollection
    {
        private readonly AlterTableCommand _command;

        public IndexesCollection(AlterTableCommand command)
        {
            _command = command;
        }

        protected override IIndex CreateItem(string name)
        {
            var command = new AlterIndexCommand(_command, name);
            _command.Add(command);
            return new ExistingIndex(command);
        }
    }
}