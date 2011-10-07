using System;
using System.Windows.Forms;
using NavigationAssistant.Core.Model;
using NavigationAssistant.Core.Utilities;

namespace NavigationAssistant.ViewModel
{
    public class AdvancedSettingsModel : BaseViewModel
    {
        #region Fields

        private Settings _settings;

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

                OnPropertyChanged("RunOnStartup");
                OnPropertyChanged("FoldersToParse");
                OnPropertyChanged("ExcludeFolderTemplates");
                OnPropertyChanged("RequireControl");
                OnPropertyChanged("RequireShift");
                OnPropertyChanged("RequireAlt");
                OnPropertyChanged("Key");
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
    }
}
