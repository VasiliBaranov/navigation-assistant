using System;

namespace NavigationAssistant.Presenters
{
    public class RequestShowingEventArgs : EventArgs
    {
        private readonly Type _presenterToShow;

        public RequestShowingEventArgs(Type presenterToShow)
        {
            _presenterToShow = presenterToShow;
        }

        public Type PresenterToShow
        {
            get { return _presenterToShow; }
        }
    }
}
