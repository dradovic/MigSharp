using System.Collections.ObjectModel;

namespace MigSharp.Generate
{
    public interface IGenerator
    {
        ReadOnlyCollection<string> Errors { get; }
        string Generate();
    }
}