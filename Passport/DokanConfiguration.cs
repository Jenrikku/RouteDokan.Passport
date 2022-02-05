using System.Collections.Generic;
using System.Configuration;

namespace RouteDokan.Passport {
    internal class DokanConfiguration {
        internal static readonly List<string> AvailableKeys = new() {
            "WiiURomFS",
            "SwitchRomFS"
        };

        internal static string GetValue(string key) {
            return ConfigurationManager.AppSettings[key] ?? string.Empty;
        }

        internal static void SetValue(string key, string value) {
            Configuration configFile =
                ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            KeyValueConfigurationCollection settings = configFile.AppSettings.Settings;

            if(settings[key] == null)
                settings.Add(key, value);
            else
                settings[key].Value = value;

            configFile.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
        }
    }
}
