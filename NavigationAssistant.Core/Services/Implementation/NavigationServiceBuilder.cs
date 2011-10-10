using System;
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
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            IFileSystemParser parser = CreateParser(settings);

            INavigatorManager primaryNavigatorManager = CreateNavigatorManager(settings.PrimaryNavigator, settings);
            List<INavigatorManager> supportedNavigatorManagers = CreateSupportedNavigatorManagers(settings, primaryNavigatorManager);

            INavigationService navigationAssistant = new NavigationService(parser,
                                                                           new MatchSearcher(),
                                                                           primaryNavigatorManager,
                                                                           supportedNavigatorManagers);

            //Warming up (to fill caches, etc)
            navigationAssistant.GetFolderMatches("temp");

            return navigationAssistant;
        }

        public void UpdateNavigationSettings(INavigationService navigationService, Settings settings)
        {
            if (navigationService == null)
            {
                throw new ArgumentNullException("navigationService");
            }

            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            INavigatorManager primaryNavigatorManager = CreateNavigatorManager(settings.PrimaryNavigator, settings);
            List<INavigatorManager> supportedNavigatorManagers = CreateSupportedNavigatorManagers(settings, primaryNavigatorManager);

            navigationService.PrimaryNavigatorManager = primaryNavigatorManager;
            navigationService.SupportedNavigatorManagers = supportedNavigatorManagers;

            CachedFileSystemParser parser = navigationService.FileSystemParser as CachedFileSystemParser;
            if (parser != null)
            {
                parser.ExcludeFolderTemplates = settings.ExcludeFolderTemplates;
                parser.FoldersToParse = settings.FoldersToParse;
            }

            //Warming up (to fill caches, etc)
            navigationService.GetFolderMatches("temp");
        }

        #endregion

        #region Non Public Methods

        private static List<INavigatorManager> CreateSupportedNavigatorManagers(Settings settings, INavigatorManager primaryNavigatorManager)
        {
            List<Navigators> additionalNavigators = new List<Navigators>(settings.SupportedNavigators);
            additionalNavigators.Remove(settings.PrimaryNavigator);

            List<INavigatorManager> supportedNavigatorManagers =
                additionalNavigators
                    .Select(navigator => CreateNavigatorManager(navigator, settings))
                    .ToList();

            supportedNavigatorManagers.Add(primaryNavigatorManager);

            return supportedNavigatorManagers;
        }

        private static IFileSystemParser CreateParser(Settings settings)
        {
            //Don't use the same file system parser for cachedParser and AsyncFileSystemParser,
            //as AsyncFileSystemParser will operate on a different thread.
            IFileSystemParser basicParser = new FileSystemParser(new FileSystemListener());
            ICacheSerializer cacheSerializer = new CacheSerializer();
            IFileSystemParser cachedParser = new CachedFileSystemParser(basicParser,
                                                                        cacheSerializer,
                                                                        new FileSystemListener(),
                                                                        new RegistryService(),
                                                                        new AsyncFileSystemParser(new FileSystemParser(new FileSystemListener())));

            cachedParser.ExcludeFolderTemplates = settings.ExcludeFolderTemplates;
            cachedParser.FoldersToParse = settings.FoldersToParse;

            return cachedParser;
        }

        private static INavigatorManager CreateNavigatorManager(Navigators navigator, Settings settings)
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
