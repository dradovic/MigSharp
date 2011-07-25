using System;
using System.Runtime.Serialization;

namespace MigSharp.Migrate
{
    [Serializable]
    public class InvalidCommandLineArgumentException : Exception
    {
        private readonly int _exitCode;

        public int ExitCode { get { return _exitCode; } }

        public InvalidCommandLineArgumentException(string message, int exitCode) :
            base(message)
        {
            _exitCode = exitCode;
        }

        public InvalidCommandLineArgumentException(string message, Exception innerException) :
            base(message, innerException)
        {
        }

        public InvalidCommandLineArgumentException(string message) :
            base(message)
        {
        }

        public InvalidCommandLineArgumentException()
        {
        }

        protected InvalidCommandLineArgumentException(SerializationInfo info, StreamingContext context) :
            base(info, context)
        {
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("ExitCode", _exitCode);
        }
    }
}