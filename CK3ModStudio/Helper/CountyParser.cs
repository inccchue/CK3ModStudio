using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            if (string.IsNullOrEmpty(filePath)) return;
            var saveDir = Path.GetDirectoryName(filePath);
            if (!File.Exists(filePath) && !string.IsNullOrEmpty(saveDir) && !Directory.Exists(saveDir)) return;
            try
            {
                var stringBuilder = new StringBuilder();

                // 遍历每个伯爵领地
                foreach (var county in Counties.OrderBy(c => c.Name))
                {
                    if (string.IsNullOrEmpty(county.Name)) continue;
                    // 기록伯爵领地名称（首字母小写，因为原파일格式是小写开头）
                    string countyName = char.ToLower(county.Name[0]) + county.Name.Substring(1);
                    stringBuilder.AppendLine($"c_{countyName} = {{");

                    // 收集所有条目并按日期分组
                    var allEntries = new Dictionary<string, List<string>>();

                    // 添加 Holder 条目
                    foreach (var holderEntry in county.HolderEntries)
                    {
                        if (!allEntries.ContainsKey(holderEntry.StartDate))
                        {
                            allEntries[holderEntry.StartDate] = new List<string>();
                        }
                        allEntries[holderEntry.StartDate].Add($"holder = {holderEntry.Holder}");
                    }

                    // 添加 Liege 条目
                    foreach (var liegeEntry in county.LiegeEntries)
                    {
                        if (!allEntries.ContainsKey(liegeEntry.StartDate))
                        {
                            allEntries[liegeEntry.StartDate] = new List<string>();
                        }
                        allEntries[liegeEntry.StartDate].Add($"liege = {liegeEntry.Liege}");
                    }

                    // 添加 Other 条目
                    foreach (var otherEntry in county.OtherEntries)
                    {
                        if (!allEntries.ContainsKey(otherEntry.StartDate))
                        {
                            allEntries[otherEntry.StartDate] = new List<string>();
                        }
                        allEntries[otherEntry.StartDate].Add(otherEntry.Content);
                    }

                    // 按日期排序并기록
                    var sortedDates = allEntries.Keys.OrderBy(date => CommonHelper.ParseDate(date)).ToList();

                    foreach (var date in sortedDates)
                    {
                        var entries = allEntries[date];

                        // 所有条目都使用大括号格式
                        stringBuilder.AppendLine($"\t{date}={{");
                        foreach (var entry in entries)
                        {
                            stringBuilder.AppendLine($"\t\t{entry}");
                        }
                        stringBuilder.AppendLine("\t}");
                    }

                    stringBuilder.AppendLine("}");
                    stringBuilder.AppendLine(); // 添加空行分隔不同的伯爵领地
                }

                // 기록파일，覆盖原有内容
                File.WriteAllText(filePath, stringBuilder.ToString(), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"파일 저장 중 오류: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static void ParseCountiesFromFile(ObservableCollection<County> Counties, string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return;
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

                        // 解析所有日期条目
                        ExtractAllEntries(countyDetails, county);

                        // 添加까지列表，不论是否有보유자 정보
                        Counties.Add(county);
                    }
                }

                // 按伯爵领名称排序
                Counties = new ObservableCollection<County>(Counties.OrderBy(c => c.Name));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"파일 파싱 중 오류: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static void ExtractAllEntries(string countyDetails, County county)
        {
            // 使用正则表达式匹配所有日期块
            string dateBlockPattern = @"(\d+\.\d+\.\d+)\s*=\s*\{([^{}]*(?:\{[^{}]*\}[^{}]*)*)\}";
            var dateBlockMatches = Regex.Matches(countyDetails, dateBlockPattern);

            foreach (Match blockMatch in dateBlockMatches)
            {
                if (blockMatch.Success && blockMatch.Groups.Count >= 3)
                {
                    string date = blockMatch.Groups[1].Value;
                    string blockContent = blockMatch.Groups[2].Value.Trim();

                    // 检查这个块是否包含holder
                    string holderPattern = @"holder\s*=\s*([^\s{}]+)";
                    Match holderMatch = Regex.Match(blockContent, holderPattern);

                    if (holderMatch.Success)
                    {
                        // 예果包含holder，添加까지HolderEntries
                        string holder = holderMatch.Groups[1].Value;
                        county.HolderEntries.Add(new HolderEntry
                        {
                            StartDate = date,
                            Holder = holder
                        });
                    }

                    string liegePattern = @"liege\s*=\s*([^\s{}]+)";
                    Match liegeMatch = Regex.Match(blockContent, liegePattern);
                    if(liegeMatch.Success)
                    {
                        // 예果包含liege，添加까지HolderEntries
                        string liege = liegeMatch.Groups[1].Value;
                        county.LiegeEntries.Add(new LiegeEntry
                        {
                            StartDate = date,
                            Liege = liege
                        });
                    }

                    if (!holderMatch.Success && !liegeMatch.Success)
                    {
                        // 예果不包含holder，添加까지OtherEntries
                        county.OtherEntries.Add(new OtherEntry
                        {
                            StartDate = date,
                            Content = blockContent
                        });
                    }
                }
            }

            // 按日期排序
            county.HolderEntries = new ObservableCollection<HolderEntry>(
                county.HolderEntries.OrderBy(h => ExtractYear(h.StartDate)));

            county.LiegeEntries = new ObservableCollection<LiegeEntry>(
                county.LiegeEntries.OrderBy(h => ExtractYear(h.StartDate)));

            county.OtherEntries = new ObservableCollection<OtherEntry>(
                county.OtherEntries.OrderBy(o => ExtractYear(o.StartDate)));
        }

        private static int ExtractYear(string dateString)
        {
            // 부터日期字符串中提取年份
            var parts = dateString.Split('.');
            if (parts.Length >= 1 && int.TryParse(parts[0], out int year))
            {
                return year;
            }
            return 0;
        }
    }
}
