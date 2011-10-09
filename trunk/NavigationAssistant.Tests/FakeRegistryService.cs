using System;
using NavigationAssistant.Core.Services;

namespace NavigationAssistant.Tests
{
    public class FakeRegistryService : IRegistryService
    {
        #region Properties

        public DateTime LastSystemShutDownTime { get; set; }

        public bool RunOnStartUp { get; set; }

        public string TotalCommanderFolder { get; set; }

        #endregion

        #region Public Methods

        public DateTime GetLastSystemShutDownTime()
        {
            return LastSystemShutDownTime;
        }

        public bool GetRunOnStartup()
        {
            return RunOnStartUp;
        }

        public void SetRunOnStartup(bool value)
        {
            RunOnStartUp = value;
        }

        public void DeleteRunOnStartup()
        {
            RunOnStartUp = false;
        }

        public string GetTotalCommanderFolder()
        {
            return TotalCommanderFolder;
        }

        #endregion
    }
}
