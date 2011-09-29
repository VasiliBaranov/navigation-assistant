using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using NavigationAssistant.PresentationModel;
using NavigationAssistant.PresentationServices;
using NavigationAssistant.PresentationServices.Implementations;

namespace NavigationAssistant.ViewModel
{
    public class SettingsModel : INotifyPropertyChanged
    {
        #region Fields

        private readonly Settings _settings;

        private ObservableCollection<NavigatorModel> _primaryNavigatorModels;

        private ObservableCollection<NavigatorModel> _supportedNavigatorModels;

        private bool _totalCommanderEnabled;

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Constructors

        public SettingsModel()
        {
            ISettingsSerializer settingsSerializer = new SettingsSerializer();
            _settings = settingsSerializer.Deserialize();

            _supportedNavigatorModels = CreateSupportedNavigatorModels(_settings);

            //Can not use the same objects, as their properties may be changed independently.
            _primaryNavigatorModels = CreatePrimaryNavigatorModels(_settings);
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

        #endregion

        #region Non Public Methods

        private ObservableCollection<NavigatorModel> CreateSupportedNavigatorModels(Settings settings)
        {
            ObservableCollection<NavigatorModel> supportedNavigatorModels = CreateNavigatorModels();

            NavigatorModel totalCommanderNavigatorModel = supportedNavigatorModels.First(sn => sn.Type == Navigators.TotalCommander);
            totalCommanderNavigatorModel.IsSelectedChanged += HandleTotalCommanderSupportChanged;

            foreach (NavigatorModel navigatorModel in supportedNavigatorModels)
            {
                navigatorModel.IsSelected = settings.SupportedNavigators.Contains(navigatorModel.Type);
            }

            return supportedNavigatorModels;
        }

        private void HandleTotalCommanderSupportChanged(object sender, System.EventArgs e)
        {
            NavigatorModel totalCommanderNavigatorModel = sender as NavigatorModel;
            if (totalCommanderNavigatorModel == null)
            {
                return;
            }

            TotalCommanderEnabled = totalCommanderNavigatorModel.IsSelected;
        }

        private ObservableCollection<NavigatorModel> CreatePrimaryNavigatorModels(Settings settings)
        {
            ObservableCollection<NavigatorModel> primaryNavigatorModels = CreateNavigatorModels();

            foreach (NavigatorModel navigator in primaryNavigatorModels)
            {
                navigator.IsSelectedChanged += HandlePrimaryNavigatorChanged;
            }

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

        private void HandlePrimaryNavigatorChanged(object sender, System.EventArgs e)
        {
            NavigatorModel primaryNavigatorModel = sender as NavigatorModel;
            if (primaryNavigatorModel == null)
            {
                return;
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
