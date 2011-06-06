using System;
using System.Diagnostics;

using MigSharp.Providers;

namespace MigSharp.Process
{
    internal class SqlDispatcher : ISqlDispatcher
    {
        private readonly ScriptingOptions _scriptingOptions;
        private readonly IProvider _provider;
        private readonly IProviderMetadata _providerMetadata;

        public SqlDispatcher(ScriptingOptions scriptingOptions, IProvider provider, IProviderMetadata providerMetadata)
        {
            if (scriptingOptions == null) throw new ArgumentNullException("scriptingOptions");

            _scriptingOptions = scriptingOptions;
            _provider = provider;
            _providerMetadata = providerMetadata;
        }

        public IDbCommandExecutor CreateExecutor(string stepName)
        {
            IDbCommandExecutor executor;
            if (_scriptingOptions.Mode == ScriptingMode.ExecuteOnly)
            {
                ISqlScripter scripter = new LoggingScripter();
                executor = new ExecutingCommandExecutor(scripter);                
            }
            else if (_scriptingOptions.Mode == ScriptingMode.ScriptOnly)
            {
                ISqlScripter scripter = new FileScripter(_scriptingOptions.TargetDirectory, stepName, _provider, _providerMetadata);
                executor = new ScriptingCommandExecutor(scripter);
            }
            else
            {
                Debug.Assert(_scriptingOptions.Mode == ScriptingMode.ScriptAndExecute, "Unknown scripting mode: " + _scriptingOptions.Mode);
                ISqlScripter loggingScripter = new LoggingScripter();
                ISqlScripter fileScripter = new FileScripter(_scriptingOptions.TargetDirectory, stepName, _provider, _providerMetadata);
                ISqlScripter scripter = new AggregateScripter(loggingScripter, fileScripter);
                executor = new ExecutingCommandExecutor(scripter);
            }
            return executor;
        }
    }
}