using System.Collections.Generic;

using MigSharp.Providers;

namespace MigSharp.Core
{
    internal interface IScriptableCommand : ICommand
    {
        IEnumerable<string> Script(IProvider provider);
    }
}