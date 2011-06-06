using System.Collections.Generic;

using MigSharp.Providers;

namespace MigSharp.Core
{
    internal interface ITranslatableCommand : ICommand
    {
        IEnumerable<string> ToSql(IProvider provider, IRuntimeContext context);
    }
}