using System;
using System.ComponentModel;
using System.Windows.Forms;
using Core.Model;
using Core.Utilities;

namespace NavigationAssistant.ViewModel
{
    public class AdvancedSettingsModel : INotifyPropertyChanged
    {
        #region Fields

        private readonly Settings _settings;

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Constructors

        public AdvancedSettingsModel(Settings settings)
        {
            _settings = settings;
        }

        #endregion

        #region Properties

        public bool IncludeHiddenFolders
        {
            get
            {
                return _settings.IncludeHiddenFolders;
            }
            set
            {
                _settings.IncludeHiddenFolders = value;
                OnPropertyChanged("IncludeHiddenFolders");
            }
        }

        public bool RunOnStartup
        {
            get
            {
                return _settings.RunOnStartup;
            }
            set
            {
                _settings.RunOnStartup = value;
                OnPropertyChanged("RunOnStartup");
            }
        }

        public int CacheUpdateIntervalInSeconds
        {
            get
            {
                return _settings.CacheUpdateIntervalInSeconds;
            }
            set
            {
                _settings.CacheUpdateIntervalInSeconds = value;
                OnPropertyChanged("CacheUpdateIntervalInSeconds");
            }
        }

        public string CacheFolder
        {
            get
            {
                return _settings.CacheFolder;
            }
            set
            {
                _settings.CacheFolder = value;
                OnPropertyChanged("CacheFolder");
            }
        }

        public string FoldersToParse
        {
            get
            {
                return StringUtility.BuildQuotedString(_settings.FoldersToParse);
            }
            set
            {
                _settings.FoldersToParse = StringUtility.ParseQuotedString(value);
                OnPropertyChanged("FoldersToParse");
            }
        }

        public string ExcludeFolderTemplates
        {
            get
            {
                return StringUtility.BuildQuotedString(_settings.ExcludeFolderTemplates);
            }
            set
            {
                _settings.ExcludeFolderTemplates = StringUtility.ParseQuotedString(value);
                OnPropertyChanged("ExcludeFolderTemplates");
            }
        }

        public bool RequireControl
        {
            get
            {
                return _settings.GlobalKeyCombination.KeyEquivalentsArePresent(KeyEquivalents.Control);
            }
            set
            {
                _settings.GlobalKeyCombination.UpdateKeyEquivalents(KeyEquivalents.Control, value);
                OnPropertyChanged("RequireControl");
            }
        }

        public bool RequireShift
        {
            get
            {
                return _settings.GlobalKeyCombination.KeyEquivalentsArePresent(KeyEquivalents.Shift);
            }
            set
            {
                _settings.GlobalKeyCombination.UpdateKeyEquivalents(KeyEquivalents.Shift, value);
                OnPropertyChanged("RequireShift");
            }
        }

        public bool RequireAlt
        {
            get
            {
                return _settings.GlobalKeyCombination.KeyEquivalentsArePresent(KeyEquivalents.Alt);
            }
            set
            {
                _settings.GlobalKeyCombination.UpdateKeyEquivalents(KeyEquivalents.Alt, value);
                OnPropertyChanged("RequireAlt");
            }
        }

        public string Key
        {
            get
            {
                return "M";//_settings.GlobalKeyCombination.KeyEquivalentsArePresent(KeyEquivalents.Alt);
            }
            set
            {
                Keys key = (Keys) Enum.Parse(typeof(Keys), value);
                _settings.GlobalKeyCombination.UpdateKeyEquivalents(new KeyEquivalents(key), true);
                OnPropertyChanged("Key");
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

        #endregion
    }
}
