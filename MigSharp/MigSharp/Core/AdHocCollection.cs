using System.Collections.Generic;

namespace MigSharp.Core
{
    internal abstract class AdHocCollection<T>
    {
        private readonly Dictionary<string, T> _items = new Dictionary<string, T>();

        public T this[string name]
        {
            get
            {
                T value;
                if (!_items.TryGetValue(name, out value))
                {
                    value = CreateItem(name);
                    _items.Add(name, value);
                }
                return value;
            }
        }

        protected abstract T CreateItem(string name);
    }
}