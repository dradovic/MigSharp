using System;
using System.Data;

namespace MigSharp.Process
{
    internal class ScriptingCommandExecutor : IDbCommandExecutor, IDisposable
    {
        private readonly ISqlScripter _sqlScripter;

        public ScriptingCommandExecutor(ISqlScripter sqlScripter)
        {
            _sqlScripter = sqlScripter;
        }

        public virtual void ExecuteNonQuery(IDbCommand command)
        {
            _sqlScripter.Script(command);
        }

        # region Disposing

        private bool _alreadyDisposed;

        ~ScriptingCommandExecutor()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if (_alreadyDisposed) return;

            try
            {
                if (isDisposing)
                {
                    IDisposable disposable = _sqlScripter as IDisposable;
                    if (disposable != null)
                    {
                        disposable.Dispose();
                    }
                }
            }
            finally
            {
                _alreadyDisposed = true;
            }
        }

        #endregion
    }
}