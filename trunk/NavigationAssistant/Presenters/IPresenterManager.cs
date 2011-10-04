using System;
using System.Collections.Generic;

namespace NavigationAssistant.Presenters
{
    public interface IPresenterManager
    {
        List<IPresenter> Presenters { get; }

        event EventHandler Exited;
    }
}
