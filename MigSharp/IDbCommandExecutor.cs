using System.Data;

namespace MigSharp
{
    /// <summary>
    /// Represents a type that can execute and script a command.
    /// </summary>
    public interface IDbCommandExecutor
    {
        /// <summary>
        /// Executes and scripts (if required) the <paramref name="command"/>.
        /// </summary>
        void ExecuteNonQuery(IDbCommand command);
    }
}