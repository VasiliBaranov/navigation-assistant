using System;
using System.ComponentModel;
using Core.Model;

namespace NavigationAssistant.ViewModel
{
    public class NavigatorModel : INotifyPropertyChanged
    {
        #region Fields

        private string _name;

        private Navigators _type;

        private bool _isSelected;

        private bool _isEnabled = true;

        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler IsSelectedChanged;

        #endregion

        #region Constructors

        public NavigatorModel(string name, Navigators type)
        {
            _name = name;
            _type = type;
        }

        #endregion

        #region Properties

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                OnPropertyChanged("Name");
            }
        }

        public Navigators Type
        {
            get
            {
                return _type;
            }
            set
            {
                _type = value;
                OnPropertyChanged("Type");
            }
        }

        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                _isSelected = value;
                OnPropertyChanged("IsSelected");
                OnIsSelectedChanged();
            }
        }

        public bool IsEnabled
        {
            get
            {
                return _isEnabled;
            }
            set
            {
                _isEnabled = value;
                OnPropertyChanged("IsEnabled");
            }
        }

        #endregion

        #region Non Public Methods

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        protected virtual void OnIsSelectedChanged()
        {
            if (IsSelectedChanged != null)
            {
                IsSelectedChanged(this, new EventArgs());
            }
        }

        #endregion
    }
}
