using MigSharp.Core.Commands;

namespace MigSharp.Core.Entities
{
    internal class SchemaCollection : AdHocCollection<IExistingSchema>, IExistingSchemaCollection
    {
        private readonly MigrateCommand _command;

        internal SchemaCollection(MigrateCommand command)
        {
            _command = command;
        }

        protected override IExistingSchema CreateItem(string name)
        {
            AlterSchemaCommand alterSchemaCommand = new AlterSchemaCommand(_command, name);
            _command.Add(alterSchemaCommand);
            return new ExistingSchema(alterSchemaCommand);
        }
    }
}