namespace MigSharp.Core.Commands
{
    internal class AlterSchemaCommand : Command
    {
        private readonly string _schema;

        public string Schema { get { return _schema; } }

        public AlterSchemaCommand(Command parent, string schema) 
            : base(parent)
        {
            _schema = schema;
        }
    }
}