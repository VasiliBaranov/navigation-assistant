using System;

namespace NavigationAssistant.Core.Services
{
    public interface IRegistryService
    {
        DateTime GetLastSystemShutDownTime();

        bool GetRunOnStartup();

        void SetRunOnStartup(bool value);

        void DeleteRunOnStartup();

        string GetTotalCommanderFolder();
    }
}
