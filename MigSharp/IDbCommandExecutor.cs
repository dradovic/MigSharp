using System.Data;

namespace MigSharp
{
    public interface IDbCommandExecutor
    {
        void ExecuteNonQuery(IDbCommand command);
    }
}