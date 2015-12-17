using System;
using System.Runtime.Serialization;

namespace MigSharp.Core.Commands
{

#pragma warning disable 1591

    [Serializable]
    public class InvalidCommandException : Exception
    {
        public InvalidCommandException(string message, Exception innerException) :
            base(message, innerException)
        {
        }

        public InvalidCommandException(string message) :
            base(message)
        {
        }

        public InvalidCommandException()
        {
        }

        protected InvalidCommandException(SerializationInfo info, StreamingContext context) :
            base(info, context)
        {
        }
    }

#pragma warning restore 1591

}