using System;
using System.Data;
using System.Linq;

namespace MigSharp.Process
{
    internal class AggregateScripter : ISqlScripter, IDisposable
    {
        private readonly ISqlScripter[] _scripters;

        public AggregateScripter(params ISqlScripter[] scripters)
        {
            _scripters = scripters;
        }

        public void Script(IDbCommand command)
        {
            foreach (ISqlScripter scripter in _scripters)
            {
                scripter.Script(command);
            }
        }

         # region Disposing

        private bool _alreadyDisposed;

        ~AggregateScripter()
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
                    foreach (IDisposable disposable in _scripters.OfType<IDisposable>())
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