namespace NMig
{
    public abstract class DbObject
    {
        private readonly string _name;

        public string Name { get { return _name; } }

        protected DbObject(string name)
        {
            _name = name;
        }
    }
}