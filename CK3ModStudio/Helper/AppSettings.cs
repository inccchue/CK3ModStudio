using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace WpfPrismFrameworkTemplate.Helper
{
    /// <summary>
    /// 轻量级持久化设置，保存在 %AppData%\CreatePeopleTool\settings.json
    /// </summary>
    public static class AppSettings
    {
        private static readonly string SettingsFile = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "CK3ModStudio", "settings.json");

        public static string LastLandedTitlesFile
        {
            get => Get("LastLandedTitlesFile");
            set => Set("LastLandedTitlesFile", value);
        }

        public static string SubmodPath
        {
            get => Get("SubmodPath");
            set => Set("SubmodPath", value);
        }

        public static string AgotModPath
        {
            get => Get("AgotModPath");
            set => Set("AgotModPath", value);
        }

        // ── 内部实现 ────────────────────────────────────────────────────

        private static string Get(string key)
        {
            var d = Load();
            string val;
            return d.TryGetValue(key, out val) ? val : null;
        }

        private static void Set(string key, string value)
        {
            var d = Load();
            d[key] = value;
            Save(d);
        }

        private static Dictionary<string, string> Load()
        {
            try
            {
                if (File.Exists(SettingsFile))
                    return JsonConvert.DeserializeObject<Dictionary<string, string>>(
                        File.ReadAllText(SettingsFile)) ?? new Dictionary<string, string>();
            }
            catch { }
            return new Dictionary<string, string>();
        }

        private static void Save(Dictionary<string, string> d)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(SettingsFile));
                File.WriteAllText(SettingsFile, JsonConvert.SerializeObject(d, Formatting.Indented));
            }
            catch { }
        }
    }
}
