using System.Collections.Generic;
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

        [XmlIgnore]
        public bool RunOnStartup { get; set; }

        #endregion
    }
}
