using System.Collections.Generic;

namespace NMig.Core
{
    internal interface ICommand
    {
        //void Add(ICommand child);
        IEnumerable<ICommand> Children { get; }
    }
}