using System;
using NavigationAssistant.Core.Utilities;

namespace NavigationAssistant.Presenters.Implementation
{
    /// <summary>
    /// Implements a base presenter.
    /// </summary>
    /// <remarks>
    /// Doesn't implement IPresenter interface to avoid difficulties with firing events from inheriting classes.
    /// </remarks>
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
