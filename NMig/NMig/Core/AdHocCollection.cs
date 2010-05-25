using System.Collections.Generic;

namespace NMig.Core
{
    internal abstract class AdHocCollection<T>
    {
        private readonly IRecorder _recorder;
        private readonly Dictionary<string, T> _items = new Dictionary<string, T>();

        internal AdHocCollection(IRecorder recorder)
        {
            _recorder = recorder;
        }

        public T this[string name]
        {
            get
            {
                T value;
                if (!_items.TryGetValue(name, out value))
                {
                    value = CreateItem(name, _recorder);
                    _items.Add(name, value);
                }
                return value;
            }
        }

        protected abstract T CreateItem(string name, IRecorder recorder);
    }
}