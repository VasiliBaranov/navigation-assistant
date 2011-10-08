using System;
using NUnit.Framework;
using NavigationAssistant.Core.Services;
using NavigationAssistant.Core.Services.Implementation;

namespace NavigationAssistant.Tests
{
    [TestFixture]
    public class RegistryServiceTests
    {
        private IRegistryService _registryService;

        [SetUp]
        public void SetUp()
        {
            _registryService = new RegistryService("temp assembly path");
            _registryService.DeleteRunOnStartup();
        }

        [TearDown]
        public void TearDown()
        {
            _registryService.DeleteRunOnStartup();
        }

        [Test]
        public void GetRunOnStartup_ForEmptyRegistry_ReturnsFalse()
        {
            bool runOnStartup = _registryService.GetRunOnStartup();

            Assert.That(runOnStartup, Is.False);
        }

        [Test]
        public void GetRunOnStartup_AfterWritingFalse_ReturnsFalse()
        {
            _registryService.SetRunOnStartup(false);
            bool runOnStartup = _registryService.GetRunOnStartup();

            Assert.That(runOnStartup, Is.False);
        }

        [Test]
        public void GetRunOnStartup_AfterWritingTrue_ReturnsTrue()
        {
            _registryService.SetRunOnStartup(true);
            bool runOnStartup = _registryService.GetRunOnStartup();

            Assert.That(runOnStartup, Is.True);
        }

        [Test]
        public void GetLastSystemShutDownTime_ReturnValueIsLessThanNow()
        {
            DateTime lastShutDownTime = _registryService.GetLastSystemShutDownTime();

            Assert.That(lastShutDownTime, Is.LessThan(DateTime.Now));
        }

        [Test]
        public void GetTotalCommanderFolder_DoesntThrowException()
        {
            Assert.DoesNotThrow(() => _registryService.GetTotalCommanderFolder());
        }
    }
}
