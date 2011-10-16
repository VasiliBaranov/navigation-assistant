using System;

namespace NavigationAssistant.Core.Services
{
    /// <summary>
    /// Defines methods modifying registry.
    /// </summary>
    public interface IRegistryService
    {
        DateTime GetLastSystemShutDownTime();

        bool GetRunOnStartup();

        void SetRunOnStartup(bool value);

        void DeleteRunOnStartup();

        string GetTotalCommanderFolder();
    }
}
