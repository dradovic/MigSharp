namespace MigSharp.Core.Entities
{
    internal abstract class AdHocCollection<T>
    {
        public T this[string name] { get { return CreateItem(name); } }

        protected abstract T CreateItem(string name);
    }
}