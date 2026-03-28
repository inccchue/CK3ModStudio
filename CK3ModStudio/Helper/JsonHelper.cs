using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using WpfPrismFrameworkTemplate.Model;

namespace WpfPrismFrameworkTemplate.Helper
{
    public class JsonHelper
    {
        private const string DATA_FILE_PATH = "families_data.json";
        private const string FILE_SETTING_PATH = "file_setting.json";

        public static ObservableCollection<Family> LoadData()
        {
            ObservableCollection<Family> loadedFamilies=new ObservableCollection<Family>();
            try
            {
                var settings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto
                };
                if (File.Exists(DATA_FILE_PATH))
                {
                    string json = File.ReadAllText(DATA_FILE_PATH);
                    loadedFamilies = JsonConvert.DeserializeObject<ObservableCollection<Family>>(json, settings) ?? new ObservableCollection<Family>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"加载数据时出错: {ex.Message}");
                // 可以在这里添加日志记录或其他错误处理
            }
            return loadedFamilies;
        }

        public static FileReadWrite LoadFileSetting()
        {
            FileReadWrite fileSetting = new FileReadWrite();
            try
            {
                
                var settings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto
                };
                if (File.Exists(FILE_SETTING_PATH))
                {
                    string json = File.ReadAllText(FILE_SETTING_PATH);
                    fileSetting = JsonConvert.DeserializeObject<FileReadWrite>(json, settings) ?? new FileReadWrite();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"加载文件设置时出错: {ex.Message}");
                // 可以在这里添加日志记录或其他错误处理
            }
            return fileSetting;
        }

        // 保存数据
        public static void SaveData(ObservableCollection<Family> familyList)
        {
            try
            {
                var settings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto,
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                    ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                };
                string json = JsonConvert.SerializeObject(familyList, Formatting.Indented, settings);
                File.WriteAllText(DATA_FILE_PATH, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"保存数据时出错: {ex.Message}");
                // 可以在这里添加日志记录或其他错误处理
            }
        }

        public static void SaveFileSetting(FileReadWrite fileSetting)
        {
            try
            {
                var settings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto,
                };
                string json = JsonConvert.SerializeObject(fileSetting, Formatting.Indented, settings);
                File.WriteAllText(FILE_SETTING_PATH, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"保存数据时出错: {ex.Message}");
                // 可以在这里添加日志记录或其他错误处理
            }
        }
    }
}
