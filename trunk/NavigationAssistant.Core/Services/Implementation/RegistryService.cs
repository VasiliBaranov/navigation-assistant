using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using Microsoft.Win32;

namespace NavigationAssistant.Core.Services.Implementation
{
    public class RegistryService : IRegistryService
    {
        public const string StartupRunParameter = "/startup";

        private readonly string _assemblyPath;

        public RegistryService()
        {
            _assemblyPath = Assembly.GetEntryAssembly().Location;
        }

        //This constructor is needed just for unit tests (where Assembly.GetEntryAssembly() is null).
        public RegistryService(string assemblyPath)
        {
            _assemblyPath = assemblyPath;
        }

        public DateTime GetLastSystemShutDownTime()
        {
            const string keyPath = @"System\CurrentControlSet\Control\Windows";
            RegistryKey key = Registry.LocalMachine.OpenSubKey(keyPath);

            const string valueName = "ShutdownTime";
            byte[] val = (byte[])key.GetValue(valueName);
            long valueAsLong = BitConverter.ToInt64(val, 0);
            return DateTime.FromFileTime(valueAsLong);
        }

        public bool GetRunOnStartup()
        {
            RegistryKey startupKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
            string navigationAssistantPath = startupKey.GetValue("NavigationAssistant") as string;

            bool runOnStartup = navigationAssistantPath != null;
            return runOnStartup;
        }

        public void SetRunOnStartup(bool value)
        {
            RegistryKey startupKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);

            if (value)
            {
                string path = String.Format(CultureInfo.InvariantCulture, "\"{0}\" \"{1}\"", _assemblyPath, StartupRunParameter);
                startupKey.SetValue("NavigationAssistant", path, RegistryValueKind.String);
            }
            else
            {
                bool valueExists = GetRunOnStartup();
                if (valueExists)
                {
                    startupKey.DeleteValue("NavigationAssistant");
                }
            }
        }

        public void DeleteRunOnStartup()
        {
            SetRunOnStartup(false);
        }

        public string GetTotalCommanderFolder()
        {
            List<RegistryKey> registryKeys = new List<RegistryKey> { Registry.LocalMachine, Registry.CurrentUser };

            foreach (RegistryKey registryKey in registryKeys)
            {
                try
                {
                    RegistryKey installDirValue = registryKey.OpenSubKey(@"Software\Ghisler\Total Commander");

                    if (installDirValue != null)
                    {
                        string installDir = installDirValue.GetValue("InstallDir") as string;
                        if (!String.IsNullOrEmpty(installDir) && Directory.Exists(installDir))
                        {
                            return installDir;
                        }
                    }
                }
                catch
                {
                    continue;
                }
            }

            return null;
        }
    }
}
