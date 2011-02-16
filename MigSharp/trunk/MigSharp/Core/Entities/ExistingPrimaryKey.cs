using MigSharp.Core.Commands;

namespace MigSharp.Core.Entities
{
    internal class ExistingPrimaryKey : IExistingPrimaryKey
    {
        private readonly AlterPrimaryKeyCommand _command;

        public ExistingPrimaryKey(AlterPrimaryKeyCommand command)
        {
            _command = command;
        }

        public void Drop()
        {
            // FEATURE: implement generic validation where providers are asked what they support and what not
            // not supported by Teradata
            _command.Add(new DropCommand(_command));
        }
    }
}