namespace MigSharp.Process
{
    internal interface ISqlDispatcher
    {
        IDbCommandExecutor CreateExecutor(string stepName);
    }
}