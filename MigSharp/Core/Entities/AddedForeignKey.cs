using System.Collections.Generic;

using MigSharp.Core.Commands;

namespace MigSharp.Core.Entities
{
    internal class AddedForeignKey : IAddedForeignKey
    {
        private readonly AddForeignKeyToCommand _addForeignKeyToCommand;

        public AddedForeignKey(AddForeignKeyToCommand addForeignKeyToCommand)
        {
            _addForeignKeyToCommand = addForeignKeyToCommand;
        }

        public IAddedForeignKey InSchema(string schemaName)
        {
            _addForeignKeyToCommand.ReferencedTableSchema = schemaName;
            return this;
        }

        public IAddedForeignKey Through(string columnName, string referencedColumnName)
        {
            _addForeignKeyToCommand.ColumnNames.Add(new KeyValuePair<string, string>(columnName, referencedColumnName));
            return this;
        }

        public IAddedForeignKey CascadeOnDelete()
        {
            _addForeignKeyToCommand.CascadeOnDelete = true;
            return this;
        }
    }
}