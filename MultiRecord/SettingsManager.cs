using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace MultiRecord
{
    public static class SettingsManager
    {
        private static Dictionary<string, string> _soundPaths = new Dictionary<string, string>();
        private static Dictionary<string, string> _settings = new Dictionary<string, string>();
        private static readonly string SettingsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MultiRecordApp");
        private static readonly string SettingsFile = Path.Combine(SettingsFolder, "settings.ini");

        static SettingsManager()
        {
            LoadSettings();
        }

        public static string GetSoundPath(string soundType)
        {
            if (_soundPaths.ContainsKey(soundType))
            {
                return _soundPaths[soundType];
            }

            // Default sound paths
            switch (soundType)
            {
                case "Beep":
                    return "sound/beep.mp3";
                case "Delete":
                    return "sound/delete.mp3";
                case "Over":
                    return "sound/over.mp3";
                default:
                    return "";
            }
        }

        public static void SetSoundPath(string soundType, string path)
        {
            _soundPaths[soundType] = path;
        }

        // General settings methods
        public static string GetSetting(string key, string defaultValue = "")
        {
            return _settings.ContainsKey(key) ? _settings[key] : defaultValue;
        }

        public static void SetSetting(string key, string value)
        {
            _settings[key] = value;
        }

        // MySQL settings
        public static string MySqlHost 
        { 
            get => GetSetting("MySqlHost", "100.76.203.31"); 
            set => SetSetting("MySqlHost", value); 
        }

        public static int MySqlPort 
        { 
            get => int.TryParse(GetSetting("MySqlPort", "3306"), out int port) ? port : 3306; 
            set => SetSetting("MySqlPort", value.ToString()); 
        }

        public static string MySqlUser 
        { 
            get => GetSetting("MySqlUser", "FootSwitch"); 
            set => SetSetting("MySqlUser", value); 
        }

        public static string MySqlPassword 
        { 
            get => GetSetting("MySqlPassword", "Qwave@dmin020890751"); 
            set => SetSetting("MySqlPassword", value); 
        }

        public static string MySqlDatabase 
        { 
            get => GetSetting("MySqlDatabase", "FootSwitch"); 
            set => SetSetting("MySqlDatabase", value); 
        }

        public static bool MySqlEnabled 
        { 
            get => bool.TryParse(GetSetting("MySqlEnabled", "true"), out bool enabled) && enabled; 
            set => SetSetting("MySqlEnabled", value.ToString()); 
        }

        // QW Record settings
        public static string CurrentQWID 
        { 
            get => GetSetting("CurrentQWID", GenerateQWID()); 
            set => SetSetting("CurrentQWID", value); 
        }

        public static string CurrentSection 
        { 
            get => GetSetting("CurrentSection", "BottomPCB"); 
            set => SetSetting("CurrentSection", value); 
        }

        public static string InstrumentSerial 
        { 
            get => GetSetting("InstrumentSerial", "SDM35HBC900652"); 
            set => SetSetting("InstrumentSerial", value); 
        }

        public static int OperatorID 
        { 
            get => int.TryParse(GetSetting("OperatorID", "1"), out int id) ? id : 1; 
            set => SetSetting("OperatorID", value.ToString()); 
        }

        private static string GenerateQWID()
        {
            return $"QW{DateTime.Now:yyyyMMdd}{new Random().Next(10, 99)}";
        }

        public static void LoadSettings()
        {
            try
            {
                if (!Directory.Exists(SettingsFolder))
                {
                    Directory.CreateDirectory(SettingsFolder);
                }

                if (File.Exists(SettingsFile))
                {
                    string[] lines = File.ReadAllLines(SettingsFile);
                    foreach (string line in lines)
                    {
                        if (line.Contains("="))
                        {
                            string[] parts = line.Split('=');
                            if (parts.Length == 2)
                            {
                                string key = parts[0].Trim();
                                string value = parts[1].Trim();
                                
                                if (key.StartsWith("Sound_"))
                                {
                                    string soundType = key.Substring(6); // Remove "Sound_" prefix
                                    _soundPaths[soundType] = value;
                                }
                                else
                                {
                                    _settings[key] = value;
                                }
                            }
                        }
                    }
                }
                else
                {
                    // Set default values if no settings file exists
                    _soundPaths["Beep"] = "sound/beep.mp3";
                    _soundPaths["Delete"] = "sound/delete.mp3";
                    _soundPaths["Over"] = "sound/over.mp3";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"เกิดข้อผิดพลาดในการโหลดการตั้งค่า: {ex.Message}", "ข้อผิดพลาด", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                
                // Set default values on error
                _soundPaths["Beep"] = "sound/beep.mp3";
                _soundPaths["Delete"] = "sound/delete.mp3";
                _soundPaths["Over"] = "sound/over.mp3";
            }
        }

        public static void SaveSettings()
        {
            try
            {
                if (!Directory.Exists(SettingsFolder))
                {
                    Directory.CreateDirectory(SettingsFolder);
                }

                using (StreamWriter writer = new StreamWriter(SettingsFile))
                {
                    writer.WriteLine("# MultiRecord Settings File");
                    writer.WriteLine("# Sound Configuration");
                    writer.WriteLine();

                    foreach (var kvp in _soundPaths)
                    {
                        writer.WriteLine($"Sound_{kvp.Key}={kvp.Value}");
                    }

                    writer.WriteLine();
                    writer.WriteLine("# General Settings");
                    foreach (var kvp in _settings)
                    {
                        writer.WriteLine($"{kvp.Key}={kvp.Value}");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"เกิดข้อผิดพลาดในการบันทึกการตั้งค่า: {ex.Message}", "ข้อผิดพลาด", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static void ResetToDefaults()
        {
            _soundPaths.Clear();
            _soundPaths["Beep"] = "sound/beep.mp3";
            _soundPaths["Delete"] = "sound/delete.mp3";
            _soundPaths["Over"] = "sound/over.mp3";
            SaveSettings();
        }
    }
}