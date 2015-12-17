using System;
using System.Collections.Generic;

using MigSharp.Providers;

namespace MigSharp.Core.Commands
{
    internal class CallCommand : Command, ITranslatableCommand
    {
        private readonly Action<IRuntimeContext> _action;

        public CallCommand(Command parent, Action<IRuntimeContext> action)
            : base(parent)
        {
            _action = action;
        }

        public IEnumerable<string> ToSql(IProvider provider, IMigrationContext context)
        {
            var runtimeContext = context as IRuntimeContext;
            if (runtimeContext != null) // the runtimeContext == null, when recording the changes to the RecordingProvider for validation
            {
                Log.Verbose(LogCategory.Sql, "Performing call-back");
                _action(runtimeContext);
            }
            yield break;
        }
    }
}