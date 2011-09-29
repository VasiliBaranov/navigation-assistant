using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using NavigationAssistant.PresentationModel;
using NavigationAssistant.PresentationServices;
using NavigationAssistant.PresentationServices.Implementations;

namespace NavigationAssistant.ViewModel
{
    public class SettingsModel : INotifyPropertyChanged
    {
        #region Fields

        private readonly ISettingsSerializer _settingsSerializer;

        private readonly Settings _settings;

        private ObservableCollection<NavigatorModel> _primaryNavigatorModels;

        private ObservableCollection<NavigatorModel> _supportedNavigatorModels;

        private bool _totalCommanderEnabled;

        private readonly ICommand _saveCommand;

        private readonly ICommand _cancelCommand;

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Constructors

        public SettingsModel()
        {
            _settingsSerializer = new SettingsSerializer();
            _settings = _settingsSerializer.Deserialize();

            _supportedNavigatorModels = CreateSupportedNavigatorModels(_settings);

            //Can not use the same objects, as their properties may be changed independently.
            _primaryNavigatorModels = CreatePrimaryNavigatorModels(_settings);

            _saveCommand = new SaveSettingsCommand(this);
            _cancelCommand = new CancelSettingsCommand();
        }

        #endregion

        #region Properties

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

        public ICommand SaveCommand
        {
            get { return _saveCommand; }
        }

        public ICommand CancelCommand
        {
            get { return _cancelCommand; }
        }

        #endregion

        #region Public Methods

        public void Save()
        {
            _settingsSerializer.Serialize(_settings);
        }

        #endregion

        #region Non Public Methods

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

        private ObservableCollection<NavigatorModel> CreateNavigatorModels()
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

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}
