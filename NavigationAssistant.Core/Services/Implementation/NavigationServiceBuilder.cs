using System.Collections.Generic;
using System.Linq;
using NavigationAssistant.Core.Model;

namespace NavigationAssistant.Core.Services.Implementation
{
    public class NavigationServiceBuilder : INavigationServiceBuilder
    {
        #region Public Methods

        public INavigationService BuildNavigationService(Settings settings)
        {
            IFileSystemParser parser = CreateParser(settings);

            IExplorerManager primaryExplorerManager = CreateExplorerManager(settings.PrimaryNavigator, settings);
            List<IExplorerManager> supportedExplorerManagers = CreateSupportedExplorerManagers(settings, primaryExplorerManager);

            INavigationService navigationAssistant = new NavigationService(parser,
                                                                           new MatchSearcher(),
                                                                           primaryExplorerManager,
                                                                           supportedExplorerManagers);

            //Warming up (to fill caches, etc)
            navigationAssistant.GetFolderMatches("temp");

            return navigationAssistant;
        }

        public void UpdateNavigationSettings(INavigationService navigationService, Settings settings)
        {
            IExplorerManager primaryExplorerManager = CreateExplorerManager(settings.PrimaryNavigator, settings);
            List<IExplorerManager> supportedExplorerManagers = CreateSupportedExplorerManagers(settings, primaryExplorerManager);

            navigationService.PrimaryExplorerManager = primaryExplorerManager;
            navigationService.SupportedExplorerManagers = supportedExplorerManagers;

            CachedFileSystemParser parser = navigationService.FileSystemParser as CachedFileSystemParser;
            if (parser != null)
            {
                parser.ExcludeFolderTemplates = settings.ExcludeFolderTemplates;
                parser.FoldersToParse = settings.FoldersToParse;
                parser.DelayIntervalInSeconds = settings.CacheUpdateDelayInSeconds;
                parser.CacheSerializer.CacheFolder = settings.CacheFolder;
            }

            //Warming up (to fill caches, etc)
            navigationService.GetFolderMatches("temp");
        }

        #endregion

        #region Non Public Methods

        private static List<IExplorerManager> CreateSupportedExplorerManagers(Settings settings, IExplorerManager primaryExplorerManager)
        {
            List<Navigators> additionalNavigators = new List<Navigators>(settings.SupportedNavigators);
            additionalNavigators.Remove(settings.PrimaryNavigator);

            List<IExplorerManager> supportedExplorerManagers =
                additionalNavigators
                    .Select(navigator => CreateExplorerManager(navigator, settings))
                    .ToList();

            supportedExplorerManagers.Add(primaryExplorerManager);

            return supportedExplorerManagers;
        }

        private static IFileSystemParser CreateParser(Settings settings)
        {
            IFileSystemParser basicParser = new FileSystemParser(new FileSystemListener());
            ICacheSerializer cacheSerializer = new CacheSerializer(settings.CacheFolder);
            IFileSystemParser cachedParser = new CachedFileSystemParser(basicParser,
                                                                        cacheSerializer,
                                                                        new FileSystemListener(),
                                                                        settings.CacheUpdateDelayInSeconds);

            cachedParser.ExcludeFolderTemplates = settings.ExcludeFolderTemplates;
            cachedParser.FoldersToParse = settings.FoldersToParse;

            return cachedParser;
        }

        private static IExplorerManager CreateExplorerManager(Navigators navigator, Settings settings)
        {
            if (navigator == Navigators.TotalCommander)
            {
                return new TotalCommanderManager(settings.TotalCommanderPath);
            }
            else
            {
                return new WindowsExplorerManager();
            }
        }

        #endregion
    }
}
