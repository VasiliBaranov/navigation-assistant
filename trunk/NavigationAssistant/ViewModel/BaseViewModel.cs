using System;
using System.ComponentModel;
using NavigationAssistant.Core.Utilities;

namespace NavigationAssistant.ViewModel
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

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

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
