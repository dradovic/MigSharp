using System.IO;

namespace MigSharp.Process
{
    internal class ScriptingOptions
    {
        private readonly DirectoryInfo _targetDirectory;
        private readonly ScriptingMode _mode;

        public DirectoryInfo TargetDirectory { get { return _targetDirectory; } }
        public ScriptingMode Mode { get { return _mode; } }

        public ScriptingOptions(ScriptingMode mode, DirectoryInfo targetDirectory)
        {
            _targetDirectory = targetDirectory;
            _mode = mode;
        }
    }
}