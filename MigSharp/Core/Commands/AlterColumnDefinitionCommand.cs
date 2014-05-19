using System.Collections.Generic;
using System.Data;

using MigSharp.Providers;

namespace MigSharp.Core.Commands
{
    internal class AlterColumnDefinitionCommand : Command, ITranslatableCommand
    {
        private readonly DbType _type;
        private readonly bool _isNullable;

        public int? Size { get; set; }
        public int? Scale { get; set; }

        public new AlterColumnCommand Parent { get { return (AlterColumnCommand)base.Parent; } }
        public object DefaultValue { get; set; }

        public AlterColumnDefinitionCommand(AlterColumnCommand parent, DbType type, bool isNullable)
            : base(parent)
        {
            _type = type;
            _isNullable = isNullable;
        }

        public IEnumerable<string> ToSql(IProvider provider, IRuntimeContext context)
        {
            return provider.AlterColumn(Parent.Parent.TableName, new Column(
                Parent.ColumnName, 
                new DataType(_type, Size, Scale), 
                _isNullable,
                DefaultValue));
        }
    }
}