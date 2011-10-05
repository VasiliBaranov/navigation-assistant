using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace NavigationAssistant.Core.Model
{
    public class Settings : ICloneable
    {
        #region Basic

        public Navigators PrimaryNavigator { get; set; }

        public List<Navigators> SupportedNavigators { get; set; }

        public string TotalCommanderPath { get; set; }

        #endregion

        #region Advanced

        public List<string> FoldersToParse { get; set; }

        public List<string> ExcludeFolderTemplates { get; set; }

        public int CacheUpdateDelayInSeconds { get; set; }

        public string CacheFolder { get; set; }

        //Keys enumeration is weird, and is serialize in a weird way.
        //E.g. "Control, Shift, M" is serialized as "LButton MButton XButton1 Back Tab Clear Return Enter A D E H I L M Shift Control"
        [XmlIgnore]
        public Keys GlobalKeyCombination { get; set; }

        public string GlobalKeyCombinationString
        {
            get
            {
                return GlobalKeyCombination.ToString();
            }
            set
            {
                GlobalKeyCombination = (Keys) Enum.Parse(typeof (Keys), value);
            } 
        }

        //This setting is set through registry
        [XmlIgnore]
        public bool RunOnStartup { get; set; }

        #endregion

        public object Clone()
        {
            Settings clone = new Settings();

            clone.PrimaryNavigator = PrimaryNavigator;
            clone.SupportedNavigators = SupportedNavigators;
            clone.TotalCommanderPath = TotalCommanderPath;
            clone.FoldersToParse = FoldersToParse;
            clone.ExcludeFolderTemplates = ExcludeFolderTemplates;
            clone.CacheUpdateDelayInSeconds = CacheUpdateDelayInSeconds;
            clone.CacheFolder = CacheFolder;
            clone.GlobalKeyCombination = GlobalKeyCombination;
            clone.RunOnStartup = RunOnStartup;

            return clone;
        }
    }
}
