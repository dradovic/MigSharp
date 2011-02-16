using System;
using System.Collections.Generic;

using MigSharp.Providers;

namespace MigSharp.Core.Commands
{
    internal class CallCommand : Command, IScriptableCommand
    {
        private readonly Action<IRuntimeContext> _action;

        public CallCommand(ICommand parent, Action<IRuntimeContext> action)
            : base(parent)
        {
            _action = action;
        }

        public IEnumerable<string> Script(IProvider provider, IRuntimeContext context)
        {
            if (context != null) // the context == null, when recording the changes to the RecordingProvider for validation
            {
                Log.Verbose(LogCategory.Sql, "Performing call-back");
                _action(context);
            }
            yield break;
        }
    }
}