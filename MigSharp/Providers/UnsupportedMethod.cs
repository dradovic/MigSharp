namespace MigSharp.Providers
{
    internal class UnsupportedMethod
    {
        private readonly string _name;
        private readonly string _message;

        public string Name { get { return _name; } }
        public string Message { get { return _message; } }

        public UnsupportedMethod(string name, string message)
        {
            _name = name;
            _message = message;
        }
    }
}