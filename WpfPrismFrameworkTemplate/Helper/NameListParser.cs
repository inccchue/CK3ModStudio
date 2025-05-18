using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows;
using Google.Protobuf.WellKnownTypes;
using WpfPrismFrameworkTemplate.Model;

namespace WpfPrismFrameworkTemplate.Helper
{
    /// <summary>
    /// 名称文件解析器类，用于解析包含名称列表的文本文件
    /// </summary>
    public class NameFileParser
    {
        /// <summary>
        /// 解析指定文件路径中的名称列表文件
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>包含解析结果的字典，键为文化名称</returns>
        public static List<CultureNames> ParseCultures(string path)
        {
            List<CultureNames> result = new List<CultureNames>();
            try
            {
                string content = File.ReadAllText(path);               

                // 查找所有文化块
                string pattern = @"name_list_(\w+)\s*=\s*{(.*?)mercenary_names\s*=";
                var matches = Regex.Matches(content, pattern, RegexOptions.Singleline);

                foreach (Match match in matches)
                {
                    string cultureName = match.Groups[1].Value;
                    string cultureBlock = match.Groups[2].Value;

                    CultureNames culture = new CultureNames { CultureName = cultureName };

                    // 解析男性名字
                    string maleNamesPattern = @"male_names\s*=\s*{(.*?)}\s*female_names";
                    Match maleMatch = Regex.Match(cultureBlock, maleNamesPattern, RegexOptions.Singleline);
                    if (maleMatch.Success)
                    {
                        string maleNamesBlock = maleMatch.Groups[1].Value;
                        culture.MaleNames = ExtractNames(maleNamesBlock);
                    }

                    // 解析女性名字
                    string femaleNamesPattern = @"female_names\s*=\s*{(.*?)}\s*dynasty_of_location_prefix";
                    Match femaleMatch = Regex.Match(cultureBlock, femaleNamesPattern, RegexOptions.Singleline);
                    if (femaleMatch.Success)
                    {
                        string femaleNamesBlock = femaleMatch.Groups[1].Value;
                        culture.FemaleNames = ExtractNames(femaleNamesBlock);
                    }

                    result.Add(culture);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("解析名称文件失败，请检查文件格式或路径是否正确。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            

            return result;
        }

        /// <summary>
        /// 从内容中提取指定类型的名称列表
        /// </summary>
        /// <param name="content">要解析的内容</param>
        /// <param name="pattern">匹配模式</param>
        /// <returns>名称列表</returns>
        private static List<string> ExtractNames(string block)
        {
            List<string> names = new List<string>();

            // 匹配所有实际的名字(删除权重和花括号)
            string cleanedBlock = Regex.Replace(block, @"^\s*\d+\s*=\s*\{|\}\s*$", "", RegexOptions.Multiline);

            // 匹配所有名字
            var nameMatches = Regex.Matches(cleanedBlock, @"\b([A-Z][a-z]+)\b");
            foreach (Match nameMatch in nameMatches)
            {
                string name = nameMatch.Groups[1].Value;
                if (!names.Contains(name))
                {
                    names.Add(name);
                }
            }

            return names;
        }

        public static string GenerateRandomName(List<CultureNames> NameSource, People targetPeople)
        {
            Random _random = new Random();
            CultureNames names = new CultureNames();
            string randomName = string.Empty;
            if (NameSource != null && NameSource.Count > 0)
            {
                if (targetPeople.Culture == "honeywiner")
                {
                    names = NameSource.FirstOrDefault(x => x.CultureName == "reachman");
                }
                else
                {
                    names = NameSource.FirstOrDefault(x => x.CultureName == targetPeople.Culture);
                }
            }

            if (names != null)
            {
                if (targetPeople.Gender == GenderType.Male && names.MaleNames.Count() > 0)
                {
                    int index = _random.Next(0, names.MaleNames.Count);
                    randomName = names.MaleNames[index];
                }
                else if (targetPeople.Gender == GenderType.Female && names.FemaleNames.Count() > 0)
                {
                    int index = _random.Next(0, names.FemaleNames.Count);
                    randomName = names.FemaleNames[index];
                }
            }

            return randomName;
        }
    }

    /// <summary>
    /// 存储单个文化的各类名称
    /// </summary>
    public class CultureNames
    {
        public string CultureName { get; set; }
        public List<string> MaleNames { get; set; } = new List<string>();
        public List<string> FemaleNames { get; set; } = new List<string>();
    }
}
