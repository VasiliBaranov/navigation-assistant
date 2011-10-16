using System;

namespace NavigationAssistant.Core.Utilities
{
    /// <summary>
    /// Represents a universal event argument, that can contain an arbitrary object.
    /// </summary>
    /// <typeparam name="T"></typeparam>
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
