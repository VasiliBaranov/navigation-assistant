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
                string path = string.Format(CultureInfo.InvariantCulture, "\"{0}\"", Assembly.GetEntryAssembly().Location);
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
                        if (!string.IsNullOrEmpty(installDir) && Directory.Exists(installDir))
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
