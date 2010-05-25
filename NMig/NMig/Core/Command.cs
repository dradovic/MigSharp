namespace NMig.Core
{
    internal abstract class Command
    {
        private readonly DbObject _target;
        public DbObject Target { get { return _target; } }

        protected Command(DbObject target)
        {
            _target = target;
        }
    }
}