﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using NavigationAssistant.Core.Model;

namespace NavigationAssistant.ViewModel
{
    /// <summary>
    /// Implements a view model for basic application settings.
    /// </summary>
    public class BasicSettingsModel : BaseViewModel
    {
        #region Fields

        private Settings _settings;

        private ObservableCollection<NavigatorModel> _primaryNavigatorModels;

        private ObservableCollection<NavigatorModel> _supportedNavigatorModels;

        private bool _totalCommanderEnabled;

        #endregion

        #region Constructors

        #endregion

        #region Properties

        public Settings Settings
        {
            get
            {
                return _settings;
            }
            set
            {
                _settings = value;

                UnsubscribeFromNavigatorModels();

                _supportedNavigatorModels = CreateSupportedNavigatorModels(_settings);

                //Can not use the same objects, as their properties may be changed independently.
                _primaryNavigatorModels = CreatePrimaryNavigatorModels(_settings);

                OnPropertyChanged("PrimaryNavigatorModels");
                OnPropertyChanged("SupportedNavigatorModels");
                OnPropertyChanged("TotalCommanderPath");
                OnPropertyChanged("TotalCommanderEnabled");
            }
        }

        public ObservableCollection<NavigatorModel> PrimaryNavigatorModels
        {
            get
            {
                return _primaryNavigatorModels;
            }
            set
            {
                _primaryNavigatorModels = value;
                OnPropertyChanged("PrimaryNavigatorModels");
            }
        }

        public ObservableCollection<NavigatorModel> SupportedNavigatorModels
        {
            get
            {
                return _supportedNavigatorModels;
            }
            set
            {
                _supportedNavigatorModels = value;
                OnPropertyChanged("SupportedNavigatorModels");
            }
        }

        public string TotalCommanderPath
        {
            get
            {
                return _settings.TotalCommanderPath;
            }
            set
            {
                _settings.TotalCommanderPath = value;
                OnPropertyChanged("TotalCommanderPath");
            }
        }

        public bool TotalCommanderEnabled
        {
            get
            {
                return _totalCommanderEnabled;
            }
            set
            {
                _totalCommanderEnabled = value;
                OnPropertyChanged("TotalCommanderEnabled");
            }
        }

        #endregion

        #region Non Public Methods

        private void UnsubscribeFromNavigatorModels()
        {
            if (_supportedNavigatorModels != null)
            {
                foreach (NavigatorModel navigatorModel in _supportedNavigatorModels)
                {
                    navigatorModel.IsSelectedChanged -= HandleSupportedNavigatorChanged;
                }
            }

            if (_primaryNavigatorModels != null)
            {
                foreach (NavigatorModel navigator in _primaryNavigatorModels)
                {
                    navigator.IsSelectedChanged -= HandlePrimaryNavigatorChanged;
                }
            }
        }

        private ObservableCollection<NavigatorModel> CreateSupportedNavigatorModels(Settings settings)
        {
            ObservableCollection<NavigatorModel> supportedNavigatorModels = CreateNavigatorModels();

            foreach (NavigatorModel navigatorModel in supportedNavigatorModels)
            {
                navigatorModel.IsSelectedChanged += HandleSupportedNavigatorChanged;

                //The handler should be already bound, as TotalCommander textbox should be properly enabled.
                navigatorModel.IsSelected = settings.SupportedNavigators.Contains(navigatorModel.Type);
            }

            return supportedNavigatorModels;
        }

        private void HandleSupportedNavigatorChanged(object sender, EventArgs e)
        {
            NavigatorModel supportedNavigatorModel = sender as NavigatorModel;
            if (supportedNavigatorModel == null)
            {
                return;
            }

            if (supportedNavigatorModel.Type == Navigators.TotalCommander)
            {
                TotalCommanderEnabled = supportedNavigatorModel.IsSelected;
            }

            bool navigatorIsSupported = _settings.SupportedNavigators.Contains(supportedNavigatorModel.Type);
            if(supportedNavigatorModel.IsSelected && !navigatorIsSupported)
            {
                _settings.SupportedNavigators.Add(supportedNavigatorModel.Type);
            }

            if(!supportedNavigatorModel.IsSelected && navigatorIsSupported)
            {
                _settings.SupportedNavigators.Remove(supportedNavigatorModel.Type);
            }
        }

        private ObservableCollection<NavigatorModel> CreatePrimaryNavigatorModels(Settings settings)
        {
            ObservableCollection<NavigatorModel> primaryNavigatorModels = CreateNavigatorModels();

            foreach (NavigatorModel navigator in primaryNavigatorModels)
            {
                navigator.IsSelectedChanged += HandlePrimaryNavigatorChanged;
            }

            //All the handlers should be bound as supported navigators should be updated correctly.
            NavigatorModel primaryNavigator = primaryNavigatorModels.First(pn => pn.Type == settings.PrimaryNavigator);
            primaryNavigator.IsSelected = true;

            return primaryNavigatorModels;
        }

        private static ObservableCollection<NavigatorModel> CreateNavigatorModels()
        {
            ObservableCollection<NavigatorModel> navigators = new ObservableCollection<NavigatorModel>
                              {
                                  new NavigatorModel("Windows Explorer", Navigators.WindowsExplorer),
                                  new NavigatorModel("Total Commander", Navigators.TotalCommander),
                              };

            return navigators;
        }

        private void HandlePrimaryNavigatorChanged(object sender, EventArgs e)
        {
            NavigatorModel primaryNavigatorModel = sender as NavigatorModel;
            if (primaryNavigatorModel == null)
            {
                return;
            }

            if (primaryNavigatorModel.IsSelected)
            {
                _settings.PrimaryNavigator = primaryNavigatorModel.Type;
            }

            NavigatorModel supportedNavigatorModel = _supportedNavigatorModels.First(sn => sn.Type == primaryNavigatorModel.Type);

            if (primaryNavigatorModel.IsSelected)
            {
                supportedNavigatorModel.IsSelected = true;
                supportedNavigatorModel.IsEnabled = false;
            }
            else
            {
                supportedNavigatorModel.IsEnabled = true;
            }
        }

        #endregion
    }
}
