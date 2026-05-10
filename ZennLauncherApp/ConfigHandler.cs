using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace ZennLauncherApp
{
    public class ConfigHandler
    {
        public static ConfigHandler Instance { get; } = new ConfigHandler();

        private readonly GlobalFunctions func = GlobalFunctions.Instance;

        public string globalConfigPath => System.IO.Path.Combine(func.dataPath, "config.json");
        public string GlobalConfigDefaultPath => System.IO.Path.Combine(func.rootPath, "config_default.json");
        public string ProfileConfigDefaultPath => System.IO.Path.Combine(func.rootPath, "profile_config_default.json");

        public string ProfileConfigPath()
        {
            try
            {
                var config = LoadGlobalConfig();

                if (config == null || config.ActiveProfile <= 0)
                {
                    return null;
                }

                string profileFolder = System.IO.Path.Combine(func.dataPath, config.ActiveProfile.ToString());
                string profileConfigPath = System.IO.Path.Combine(profileFolder, "profile_config.json");

                if (!System.IO.Directory.Exists(profileFolder))
                {
                    System.IO.Directory.CreateDirectory(profileFolder);
                }

                return profileConfigPath;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Profile Path Error: {ex.Message}");
                return null;
            }
        }

        public ConfigHandler()
        {

        }

        public void CreateDefaultCfg()
        {
            if (!File.Exists(GlobalConfigDefaultPath))
            {
                var defaultGlobalConfig = new GlobalConfigModel
                {
                    Build = func.build,
                    Version = func.version,
                    ActiveProfile = -1,
                    SessionToken = "",
                    DefaultLanguage = "English"
                };

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                string jsonString = JsonSerializer.Serialize(defaultGlobalConfig, options);

                File.WriteAllText(GlobalConfigDefaultPath, jsonString);
            }


            if (!File.Exists(ProfileConfigDefaultPath))
            {
                var defaultProfileConfig = new ProfileConfigModel();

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                string jsonString = JsonSerializer.Serialize(defaultProfileConfig, options);
                File.WriteAllText(ProfileConfigDefaultPath, jsonString);
            }
        }

        public void SaveGlobalConfig(GlobalConfigModel globalData)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(globalData, options);

            File.WriteAllText(globalConfigPath, json);
        }

        public GlobalConfigModel LoadGlobalConfig()
        {
            if (!File.Exists(globalConfigPath))
            {
                var defaultGlobalConfig = new GlobalConfigModel
                {
                    Build = func.build,
                    Version = func.version,
                    ActiveProfile = -1,
                    SessionToken = "",
                    DefaultLanguage = "English"
                };

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                string jsonString = JsonSerializer.Serialize(defaultGlobalConfig, options);

                File.WriteAllText(globalConfigPath, jsonString);

                return defaultGlobalConfig;
            }

            string json = File.ReadAllText(globalConfigPath);
            var existingGlobalConfig = JsonSerializer.Deserialize<GlobalConfigModel>(json);
            SaveGlobalConfig(existingGlobalConfig);

            return existingGlobalConfig;
        }

        public void SaveProfileConfig(ProfileConfigModel profileData)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(profileData, options);

            jsonString = jsonString.Replace("\"Interface\":", "\n  \"Interface\":");
            jsonString = jsonString.Replace("\"Folders\":", "\n  \"Folders\":");

            File.WriteAllText(ProfileConfigPath(), jsonString);
        }

        public ProfileConfigModel LoadProfileConfig()
        {
            string profileConfigPath = this.ProfileConfigPath();

            if (!File.Exists(profileConfigPath))
            {
                var defaultProfileConfig = new ProfileConfigModel();

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                string jsonString = JsonSerializer.Serialize(defaultProfileConfig, options);

                jsonString = jsonString.Replace("\"Interface\":", "\n  \"Interface\":");
                jsonString = jsonString.Replace("\"Folders\":", "\n  \"Folders\":");

                File.WriteAllText(profileConfigPath, jsonString);

                return defaultProfileConfig;
            }

            string json = File.ReadAllText(profileConfigPath);
            var existingProfileConfig = JsonSerializer.Deserialize<ProfileConfigModel>(json);
            SaveProfileConfig(existingProfileConfig);

            return existingProfileConfig;
        }
    }  
}
