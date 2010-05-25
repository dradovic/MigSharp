using System;

namespace NMig
{
    internal class ItemAddedEventArgs<T> : EventArgs
    {
        private readonly T _item;

        public T Item { get { return _item; } }

        public ItemAddedEventArgs(T item)
        {
            _item = item;
        }
    }
}