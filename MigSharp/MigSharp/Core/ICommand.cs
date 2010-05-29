using System.Collections.Generic;

namespace MigSharp.Core
{
    internal interface ICommand
    {
        //void Add(ICommand child);
        IEnumerable<ICommand> Children { get; }
    }
}