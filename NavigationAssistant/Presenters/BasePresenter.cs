using System;
using NavigationAssistant.Utilities;

namespace NavigationAssistant.Presenters
{
    public abstract class BasePresenter
    {
        protected void FireEvent(EventHandler handler)
        {
            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }

        protected void FireEvent<T>(EventHandler<T> handler, T args) where T : EventArgs
        {
            if (handler != null)
            {
                handler(this, args);
            }
        }

        protected void FireEvent<T>(EventHandler<ItemEventArgs<T>> handler, T args)
        {
            if (handler != null)
            {
                handler(this, new ItemEventArgs<T>(args));
            }
        }
    }
}
