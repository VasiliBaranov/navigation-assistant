using System;
using System.Collections.Generic;

namespace NavigationAssistant.Presenters
{
    /// <summary>
    /// Defines methods for a presenters manager (see Mediator pattern).
    /// </summary>
    public interface IPresenterManager
    {
        List<IPresenter> Presenters { get; }

        event EventHandler Exited;
    }
}
