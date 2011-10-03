using System;

namespace NavigationAssistant.Utilities
{
    public class ItemEventArgs<T> : EventArgs
    {
        private readonly T _item;

        public ItemEventArgs(T item)
        {
            _item = item;
        }

        public T Item
        {
            get { return _item; }
        }
    }
}
