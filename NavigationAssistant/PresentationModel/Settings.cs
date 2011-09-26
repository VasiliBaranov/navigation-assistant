using System.Collections.Generic;

namespace NavigationAssistant.PresentationModel
{
    public class Settings
    {
        public List<string> FoldersToParse { get; set; }

        public bool IncludeHiddenFolders { get; set; }

        public List<string> ExcludeFolderTemplates { get; set; }

        public Navigators PrimaryNavigator { get; set; }

        public List<Navigators> AdditionalNavigators { get; set; }

        public string TotalCommanderPath { get; set; }

        public int CacheUpdateIntervalInSeconds { get; set; }

        public string CacheFolder { get; set; }
    }
}
