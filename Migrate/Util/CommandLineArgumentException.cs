using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace MigSharp.Migrate.Util
{
    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")] // for users of this exception to provide name and value
    [Serializable]
    internal class CommandLineArgumentException : Exception
    {
        private readonly string _name;
        /// <summary>
        /// Gets the name of the argument that caused the exception.
        /// </summary>
        public string Name { get { return _name; } }

        private readonly string _value;
        /// Gets the value of the argument that caused the exception.
        public string Value { get { return _value; } }

        public CommandLineArgumentException(string message, string name, string value) :
            base(message)
        {
            if (name == null) throw new ArgumentNullException("name");
            if (value == null) throw new ArgumentNullException("value");

            _name = name;
            _value = value;
        }

        public CommandLineArgumentException(string message) :
            base(message)
        {
        }

        protected CommandLineArgumentException(SerializationInfo info, StreamingContext context) :
            base(info, context)
        {
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Name", _name);
            info.AddValue("Value", _value);
        }
    }
}