using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharp.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UX.Lib2;
using UX.Lib2.Cloud.Logger;
using UX.Lib2.Models;

namespace JaneStreetAV.Models.Config
{
    public static class ConfigManager
    {
        private static SystemConfig _config;

        /// <summary>
        /// Create a system config from a config file.
        /// </summary>
        static ConfigManager()
        {
            CrestronConsole.AddNewConsoleCommand(UpgradeConfigFile, "ConfigSave", "Upgrade the current loaded config with any new parameters", ConsoleAccessLevelEnum.AccessOperator);
            CrestronConsole.AddNewConsoleCommand(ConsoleGetRoomName, "GetRoomName", "Get room name for Room ID", ConsoleAccessLevelEnum.AccessOperator);
            CrestronConsole.AddNewConsoleCommand(
                parameters =>
                    CrestronConsole.ConsoleCommandResponse("\r\n" + Debug.AnsiBlue +
                                                           JToken.FromObject(Config).ToString(Formatting.Indented) +
                                                           Debug.AnsiReset),
                "PrintCurrentConfig", "Print the current loaded config", ConsoleAccessLevelEnum.AccessOperator);
            CrestronConsole.AddNewConsoleCommand(parameters =>
            {
                try
                {
                    var types = GetTemplateTypes();

                    var type = types.FirstOrDefault(t => t.Name.ToLower() == parameters.ToLower());

                    if (type != null)
                    {
                        CrestronConsole.ConsoleCommandResponse("Creating config type: {0}\r", type.FullName);

                        var configCtor = type.GetConstructor(new CType[] {});
                        var config = (SystemConfig) configCtor.Invoke(new object[] {});

                        CrestronConsole.ConsoleCommandResponse("Created config: {0}\r", config.Name);

                        CrestronConsole.ConsoleCommandResponse("Looking for existing config at {0}\r", FilePath);

                        if (File.Exists(FilePath))
                        {
                            var backupPath = FilePath + ".old";
                            if (File.Exists(backupPath))
                            {
                                File.Delete(backupPath);
                            }
                            File.Move(FilePath, backupPath);
                            CrestronConsole.ConsoleCommandResponse("Moved existing config to " + backupPath + "\r");

                        }

                        Save(config, FilePath);
                        _config = config;

                        CrestronConsole.ConsoleCommandResponse("Created new config ok, restart application to load\r");
                        return;
                    }

                    CrestronConsole.ConsoleCommandResponse("Unknown config type: {0}\r", parameters);
                }
                catch (Exception e)
                {
                    CrestronConsole.ConsoleCommandResponse("Error: {0}\r", e.Message);
                }
            }, "CreateConfig", "Create a default config", ConsoleAccessLevelEnum.AccessOperator);

            if (File.Exists(FilePath))
            {
                Debug.WriteInfo("Config file path", FilePath);
            }
            else
            {
                Debug.WriteInfo("Config file path", FilePathDefault);
            }
        }

        public static string FilePath = SystemBase.AppStoragePath + "\\config.json";
        public static string FilePathDefault = SystemBase.AppStoragePath + "\\default_config.json";
        private static CTimer _saveTimer;

        private static SystemConfig LoadFromFile(string filePath)
        {
            // Open, read and close the file
            var configString = "";
            using (var file = new StreamReader(filePath, Encoding.UTF8))
            {
                configString = file.ReadToEnd();
                file.Close();
            }

            try
            {
                var config = JsonConvert.DeserializeObject<SystemConfig>(configString);
                Debug.WriteSuccess("Config Loaded", "System type is \"{0}\"", config.SystemType);

                if (filePath == FilePathDefault)
                {
                    config.IsDefault = true;
                }
                else
                {
                    config.IsDefault = false;
                }

                config.ConfigPath = filePath;

                if (config.IsDefault)
                {
                    Debug.WriteWarn("DEFAULT CONFIG", "The default file path has been loaded");
                }

                var configChanged = false;

                // changes to config here.

                if (configChanged)
                {
                    Save(config, FilePath);
                }

                return config;
            }
            catch (Exception e)
            {
                // Catch the error if the data does not parse ok
                CloudLog.Exception(e, "Could not deserialize config");

                return new DefaultConfig();
            }
        }

        public static IEnumerable<CType> GetTemplateTypes()
        {
            return
                Assembly.GetExecutingAssembly()
                    .GetTypes()
                    .Where(t => t.FullName.Contains("Models.Config.Templates"))
                    .ToList();
        }

        public static SystemConfig Config
        {
            get
            {
                if (_config != null) return _config;

                // Load config file if it exists
                if (File.Exists(FilePath))
                {
                    _config = LoadFromFile(FilePath);
                }
                else if (File.Exists(FilePathDefault))
                {
                    _config = LoadFromFile(FilePathDefault);
                }
                else
                {
                    Debug.WriteWarn("Could not find user config file. Setting up default config ...");
                    CloudLog.Warn("Could not find config file at {0}, Creating default config", FilePath);

                    var defaultConfig = new DefaultConfig();
                    Save(defaultConfig, FilePathDefault);
                    _config = defaultConfig;
                }

                return _config;
            }
        }

        public static object GetCustomValue(string key)
        {
            var values = Config.ValueStore;
            if (values == null || !values.ContainsKey(key)) return null;
            return values[key];
        }

        public static void SetCustomValue(string key, object value)
        {
            if (Config.ValueStore == null)
            {
                Config.ValueStore = new Dictionary<string, object>();
            }

            Config.ValueStore[key] = value;
#if DEBUG
            Debug.WriteInfo("SetCustomValue", "{0} = {1}", key, value);
#endif
            if (_saveTimer != null && !_saveTimer.Disposed)
            {
#if DEBUG
                Debug.WriteInfo("Clearing save timer");
#endif
                _saveTimer.Stop();
                _saveTimer.Dispose();
            }
            _saveTimer = new CTimer(specific =>
            {
#if DEBUG
                Debug.WriteSuccess("Saving config on timer event");
#endif
                Save(Config, FilePath);
            }, 60000);
        }

        public static void Save(SystemConfig config, string path)
        {
            try
            {
                var systemConfig = config;
                if (systemConfig != null) systemConfig.ConfigPath = path;

                string configData;

                using (var sw = new StringWriter())
                {
                    using (var jw = new JsonTextWriter(sw))
                    {
                        jw.Formatting = Formatting.Indented;
                        jw.WriteWhitespace(sw.NewLine);
                        jw.WriteRawValue(JsonConvert.SerializeObject(config, Formatting.Indented));
                        configData = sw.ToString();
                    }
                }

                using (var newFile = new StreamWriter(path, false, Encoding.UTF8))
                {
                    newFile.Write(configData);
                    newFile.Close();
                }

                _config = null;
            }
            catch (Exception e)
            {
                CloudLog.Error("Error saving config file, {0}", e.Message);
                return;
            }
        }

        static void UpgradeConfigFile(object o)
        {
            CrestronConsole.ConsoleCommandResponse("Saving {0} ...", FilePath);
            Config.IsDefault = false;
            Save(Config, FilePath);
        }

        static void ConsoleGetRoomName(string argsString)
        {
            try
            {
                var args = argsString.Split(',');
                var roomIndex = Config.Rooms.IndexOf(Config.Rooms.First(r => r.Id == uint.Parse(args[0])));
                CrestronConsole.ConsoleCommandResponse("Room name for ID {0} is {1}",
                    Config.Rooms[roomIndex].Id,
                    Config.Rooms[roomIndex].Name);
            }
            catch (Exception e)
            {
                CrestronConsole.ConsoleCommandResponse("Error {0}", e.Message);
            }
        }
    }
}