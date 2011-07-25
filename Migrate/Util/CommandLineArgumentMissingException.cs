using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace MigSharp.Migrate.Util
{
    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")] // for users of this exception to provide name and value
    [Serializable]
    internal class CommandLineArgumentMissingException : CommandLineArgumentException
    {
        public CommandLineArgumentMissingException(string name, string value) :
            base(string.Format("Missing command line argument: {0}", name), name, value)
        {
        }

        protected CommandLineArgumentMissingException(SerializationInfo info, StreamingContext context) :
            base(info, context)
        {
        }
    }
}