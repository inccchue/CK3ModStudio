using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Metrics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using WpfPrismFrameworkTemplate.Model;

namespace WpfPrismFrameworkTemplate.Helper
{
    public class CountyParser
    {
        public static void SaveCountiesToFile(ObservableCollection<County> Counties, string filePath)
        {
            try
            {
                // 首先读取原始文件内容
                string originalContent = File.ReadAllText(filePath);

                // 逐个处理每个伯爵领地
                foreach (var county in Counties)
                {
                    // 为当前伯爵领准备新的持有者信息文本
                    StringBuilder holderEntriesText = new StringBuilder();
                    foreach (var entry in county.HolderEntries)
                    {
                        holderEntriesText.AppendLine($"\t{entry.StartDate}={{\n\t\tholder={entry.Holder}\n\t}}");
                    }

                    // 构建匹配当前伯爵领的正则表达式模式
                    string countyNameLower = char.ToLower(county.Name[0]) + county.Name.Substring(1);
                    string countyPattern = @"c_" + countyNameLower + @"\s*=\s*\{(.*?)(?=c_\w+\s*=|\z)";

                    // 准备新的伯爵领文本
                    string newCountyText = $"c_{countyNameLower} = {{\n{holderEntriesText}}}\n\n";

                    // 检查伯爵领是否存在于原文件中
                    Match countyMatch = Regex.Match(originalContent, countyPattern, RegexOptions.Singleline);
                    if (countyMatch.Success)
                    {
                        // 如果存在，替换内容
                        originalContent = Regex.Replace(originalContent, countyPattern, newCountyText, RegexOptions.Singleline);
                    }
                    else
                    {
                        // 如果不存在，添加到文件末尾
                        originalContent += newCountyText;
                    }
                }

                // 将更新后的内容写回文件
                File.WriteAllText(filePath, originalContent);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存文件时出错: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public static void ParseCountiesFromFile(ObservableCollection<County> Counties,string filePath)
        {
            try
            {
                Counties.Clear();
                string content = File.ReadAllText(filePath);

                // 使用正则表达式匹配所有伯爵领地及其信息
                string pattern = @"c_(\w+)\s*=\s*\{(.*?)(?=c_\w+\s*=|\z)";
                var matches = Regex.Matches(content, pattern, RegexOptions.Singleline);

                foreach (Match match in matches)
                {
                    if (match.Success && match.Groups.Count >= 3)
                    {
                        string countyName = match.Groups[1].Value;
                        string countyDetails = match.Groups[2].Value;

                        // 首字母大写
                        countyName = char.ToUpper(countyName[0]) + countyName.Substring(1);

                        // 创建新的伯爵领对象
                        var county = new County { Name = countyName };

                        // 解析持有者信息
                        ExtractHolderEntries(countyDetails, county);

                        // 只有当有持有者信息时才添加到列表
                        if (county.HolderEntries.Count > 0)
                        {
                            Counties.Add(county);
                        }
                    }
                }

                // 按伯爵领名称排序
                Counties = new ObservableCollection<County>(Counties.OrderBy(c => c.Name));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"解析文件时出错: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static void ExtractHolderEntries(string countyDetails, County county)
        {
            // 使用正则表达式匹配所有日期和持有者信息
            string pattern = @"(\d+\.\d+\.\d+)\s*=\s*\{\s*holder\s*=\s*([^\s{}]+)\s*\}";
            var matches = Regex.Matches(countyDetails, pattern);

            foreach (Match match in matches)
            {
                if (match.Success && match.Groups.Count >= 3)
                {
                    string date = match.Groups[1].Value;
                    string holder = match.Groups[2].Value;

                    county.HolderEntries.Add(new HolderEntry
                    {
                        StartDate = date,
                        Holder = holder
                    });
                }
            }

            // 按日期排序
            county.HolderEntries = new ObservableCollection<HolderEntry>(
                county.HolderEntries.OrderBy(h => ExtractYear(h.StartDate)));
        }

        private static int ExtractYear(string dateString)
        {
            // 从日期字符串中提取年份
            var parts = dateString.Split('.');
            if (parts.Length >= 1 && int.TryParse(parts[0], out int year))
            {
                return year;
            }
            return 0;
        }
    }
}
