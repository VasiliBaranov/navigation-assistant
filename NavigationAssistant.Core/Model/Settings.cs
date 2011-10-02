using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace Core.Model
{
    public class Settings
    {
        #region Basic

        public Navigators PrimaryNavigator { get; set; }

        public List<Navigators> SupportedNavigators { get; set; }

        public string TotalCommanderPath { get; set; }

        #endregion

        #region Advanced

        public List<string> FoldersToParse { get; set; }

        public bool IncludeHiddenFolders { get; set; }

        public List<string> ExcludeFolderTemplates { get; set; }

        public int CacheUpdateIntervalInSeconds { get; set; }

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
    }
}
