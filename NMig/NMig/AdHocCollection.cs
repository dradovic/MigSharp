using System.Collections.Generic;

namespace NMig
{
    internal abstract class AdHocCollection<T>
    {
        private readonly IRecorder _recorder;
        private readonly Dictionary<string, T> _items = new Dictionary<string, T>();

        internal AdHocCollection(IRecorder recorder)
        {
            _recorder = recorder;
        }

        //internal event EventHandler<ItemAddedEventArgs<T>> ItemAdded;

        public T this[string name]
        {
            get
            {
                T value;
                if (!_items.TryGetValue(name, out value))
                {
                    value = CreateItem(name, _recorder);
                    _items.Add(name, value);
                    //OnItemAdded(new ItemAddedEventArgs<T>(value));
                }
                return value;
            }
        }

        protected abstract T CreateItem(string name, IRecorder recorder);

        //private void OnItemAdded(ItemAddedEventArgs<T> e)
        //{
        //    EventHandler<ItemAddedEventArgs<T>> tmp = ItemAdded;
        //    if (tmp != null)
        //    {
        //        tmp(this, e);
        //    }
        //}
    }
}