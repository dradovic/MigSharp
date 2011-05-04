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

        public IAddedForeignKey Through(string columnName, string referencedColumnName)
        {
            _addForeignKeyToCommand.ColumnNames.Add(new KeyValuePair<string, string>(columnName, referencedColumnName));
            return this;
        }
    }
}