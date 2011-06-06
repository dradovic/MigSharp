using System.Data;

namespace MigSharp.Process
{
    internal interface ISqlScripter
    {
        void Script(IDbCommand command);
    }
}