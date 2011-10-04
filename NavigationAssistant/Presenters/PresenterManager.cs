using System;
using System.Collections.Generic;

namespace NavigationAssistant.Presenters
{
    public class PresenterManager : BasePresenter, IPresenterManager
    {
        private readonly List<IPresenter> _presenters;

        public event EventHandler Exited;

        public List<IPresenter> Presenters
        {
            get { return _presenters; }
        }

        public PresenterManager(List<IPresenter> presenters)
        {
            _presenters = presenters;

            foreach (IPresenter presenter in _presenters)
            {
                presenter.SettingsChanged += HandleSettingsChanged;
                presenter.RequestWindowShow += HandleRequestWindowShow;
                presenter.Exited += HandleExited;
            }
        }

        private void HandleExited(object sender, EventArgs e)
        {
            foreach (IPresenter presenter in _presenters)
            {
                presenter.SettingsChanged -= HandleSettingsChanged;
                presenter.RequestWindowShow -= HandleRequestWindowShow;
                presenter.Exited -= HandleExited;

                presenter.Dispose();
            }

            FireEvent(Exited);
        }

        private void HandleRequestWindowShow(object sender, Utilities.ItemEventArgs<Type> e)
        {
            foreach (IPresenter presenter in _presenters)
            {
                if (presenter.GetType() == e.Item)
                {
                    presenter.Show();
                }
            }
        }

        private void HandleSettingsChanged(object sender, Utilities.ItemEventArgs<Core.Model.Settings> e)
        {
            foreach (IPresenter presenter in _presenters)
            {
                presenter.UpdateSettings(e.Item);
            }
        }
    }
}
