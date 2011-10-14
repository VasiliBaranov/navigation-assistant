using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using NavigationAssistant.Core.Model;
using NavigationAssistant.Core.Services;
using NavigationAssistant.Core.Services.Implementation;

namespace NavigationAssistant.Tests
{
    [TestFixture]
    class NavigationServiceBuilderTests
    {
        private INavigationServiceBuilder _navigationServiceBuilder;
        private Mock<INavigationService> _navigationServiceMock;
        private INavigationService _service;

        [SetUp]
        public void SetUp()
        {
            _navigationServiceBuilder = new NavigationServiceBuilder(false);
            _navigationServiceMock = new Mock<INavigationService>();
            _navigationServiceMock.SetupProperty(service => service.PrimaryNavigatorManager);
            _navigationServiceMock.SetupProperty(service => service.SupportedNavigatorManagers);

            _service = _navigationServiceMock.Object;
        }

        [Test]
        public void UpdateNavigationSettings_SettingsIncludeWindowsExplorer_ServiceSupportsWindowsExplorer()
        {
            Settings settings = new Settings
                                    {
                                        SupportedNavigators = new List<Navigators> { Navigators.WindowsExplorer },
                                        PrimaryNavigator = Navigators.WindowsExplorer
                                    };
            _navigationServiceBuilder.UpdateNavigationSettings(_service, settings);

            List<Type> navigatorTypes = _service.SupportedNavigatorManagers.Select(m => m.GetType()).ToList();
            Assert.That(navigatorTypes, Is.EquivalentTo(new List<Type> { typeof(WindowsExplorerManager) }));
        }

        [Test]
        public void UpdateNavigationSettings_SettingsSupportTotalCommander_ServiceSupportsTotalCommander()
        {
            Settings settings = new Settings
                                    {
                                        SupportedNavigators = new List<Navigators> { Navigators.TotalCommander },
                                        PrimaryNavigator = Navigators.TotalCommander,
                                        TotalCommanderPath = "TotalCmd.exe"
                                    };
            _navigationServiceBuilder.UpdateNavigationSettings(_service, settings);

            List<Type> navigatorTypes = _service.SupportedNavigatorManagers.Select(m => m.GetType()).ToList();
            Assert.That(navigatorTypes, Is.EquivalentTo(new List<Type> { typeof(TotalCommanderManager) }));
        }

        [Test]
        public void UpdateNavigationSettings_SettingsSupportTotalCommanderAndPrimaryNavigatorIsWindowsExplorer_ServiceSupportsTotalCommanderAndWindowsExplorer()
        {
            Settings settings = new Settings
            {
                SupportedNavigators = new List<Navigators> { Navigators.TotalCommander },
                PrimaryNavigator = Navigators.WindowsExplorer,
                TotalCommanderPath = "TotalCmd.exe"
            };
            _navigationServiceBuilder.UpdateNavigationSettings(_service, settings);

            List<Type> navigatorTypes = _service.SupportedNavigatorManagers.Select(m => m.GetType()).ToList();
            Assert.That(navigatorTypes, Is.EquivalentTo(new List<Type> { typeof(TotalCommanderManager), typeof(WindowsExplorerManager) }));
        }

        [Test]
        public void UpdateNavigationSettings_SettingsSupportTotalCommanderAndWindowsExplorer_ServiceSupportsTotalCommanderAndWindowsExplorer()
        {
            Settings settings = new Settings
                                    {
                                        SupportedNavigators = new List<Navigators>
                                                                  {
                                                                      Navigators.TotalCommander, 
                                                                      Navigators.WindowsExplorer
                                                                  },
                                        TotalCommanderPath = "TotalCmd.exe",
                                        PrimaryNavigator = Navigators.WindowsExplorer
                                    };
            _navigationServiceBuilder.UpdateNavigationSettings(_service, settings);

            List<Type> navigatorTypes = _service.SupportedNavigatorManagers.Select(m => m.GetType()).ToList();
            Assert.That(navigatorTypes, Is.EquivalentTo(new List<Type> { typeof(WindowsExplorerManager), typeof(TotalCommanderManager) }));
        }

        [Test]
        public void UpdateNavigationSettings_SettingsDoNotSupportTotalCommander_TotalCommanderRemoved()
        {
            _service.SupportedNavigatorManagers = new List<INavigatorManager> { new TotalCommanderManager("TotalCmd.exe") };
            Settings settings = new Settings
                                    {
                                        SupportedNavigators = new List<Navigators> { Navigators.WindowsExplorer },
                                        TotalCommanderPath = "TotalCmd.exe",
                                        PrimaryNavigator = Navigators.WindowsExplorer
                                    };
            _navigationServiceBuilder.UpdateNavigationSettings(_service, settings);

            List<Type> navigatorTypes = _service.SupportedNavigatorManagers.Select(m => m.GetType()).ToList();
            Assert.That(navigatorTypes, Is.EquivalentTo(new List<Type> { typeof(WindowsExplorerManager) }));
        }

        [Test]
        public void UpdateNavigationSettings_SettingsPrimaryNavigatorIsTotalCommander_ServicePrimaryNavigatorIsTotalCommander()
        {
            Settings settings = new Settings
                                    {
                                        PrimaryNavigator = Navigators.TotalCommander,
                                        SupportedNavigators = new List<Navigators>(),
                                        TotalCommanderPath = "TotalCmd.exe"
                                    };
            _navigationServiceBuilder.UpdateNavigationSettings(_service, settings);

            Assert.That(_service.PrimaryNavigatorManager.GetType(), Is.EqualTo(typeof(TotalCommanderManager)));
        }

        [Test]
        public void UpdateNavigationSettings_SettingsPrimaryNavigatorIsWindowsExplorer_ServicePrimaryNavigatorIsWindowsExplorer()
        {
            Settings settings = new Settings
                                    {
                                        PrimaryNavigator = Navigators.WindowsExplorer,
                                        SupportedNavigators = new List<Navigators>(),
                                    };
            _navigationServiceBuilder.UpdateNavigationSettings(_service, settings);

            Assert.That(_service.PrimaryNavigatorManager.GetType(), Is.EqualTo(typeof(WindowsExplorerManager)));
        }
    }
}
