using System;
using System.ComponentModel;
using System.Windows.Forms;
using NavigationAssistant.Core.Model;
using NavigationAssistant.Core.Utilities;

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

        public int CacheUpdateDelayInMinutes
        {
            get
            {
                return _settings.CacheUpdateDelayInSeconds / 60;
            }
            set
            {
                _settings.CacheUpdateDelayInSeconds = value * 60;
                OnPropertyChanged("CacheUpdateDelayInSeconds");
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
                return EnumUtility.IsPresent(_settings.GlobalKeyCombination, Keys.Control);
            }
            set
            {
                _settings.GlobalKeyCombination = EnumUtility.UpdateKey(_settings.GlobalKeyCombination, Keys.Control, value);
                OnPropertyChanged("RequireControl");
            }
        }

        public bool RequireShift
        {
            get
            {
                return EnumUtility.IsPresent(_settings.GlobalKeyCombination, Keys.Shift);
            }
            set
            {
                _settings.GlobalKeyCombination = EnumUtility.UpdateKey(_settings.GlobalKeyCombination, Keys.Shift, value);
                OnPropertyChanged("RequireShift");
            }
        }

        public bool RequireAlt
        {
            get
            {
                return EnumUtility.IsPresent(_settings.GlobalKeyCombination, Keys.Alt);
            }
            set
            {
                _settings.GlobalKeyCombination = EnumUtility.UpdateKey(_settings.GlobalKeyCombination, Keys.Alt, value);
                OnPropertyChanged("RequireAlt");
            }
        }

        public string Key
        {
            get
            {
                return EnumUtility.ExtractNotModifiedKey(_settings.GlobalKeyCombination).ToString();
            }
            set
            {
                //Remove old key code
                Keys oldKeyCode = EnumUtility.ExtractNotModifiedKey(_settings.GlobalKeyCombination);
                Keys modifiers = EnumUtility.RemoveKey(_settings.GlobalKeyCombination, oldKeyCode);

                Keys key = (Keys) Enum.Parse(typeof(Keys), value);
                _settings.GlobalKeyCombination = EnumUtility.AddKey(modifiers, key);
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
