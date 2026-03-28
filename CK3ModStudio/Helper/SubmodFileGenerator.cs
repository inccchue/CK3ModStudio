using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using WpfPrismFrameworkTemplate.Model;

namespace WpfPrismFrameworkTemplate.Helper
{
    public class SubmodGeneratorResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public List<string> GeneratedFiles { get; set; }

        public SubmodGeneratorResult()
        {
            GeneratedFiles = new List<string>();
        }
    }

    public class SubmodFileGenerator
    {
        private readonly string _submodPath;
        private readonly string _agotModPath;

        public SubmodFileGenerator(string submodPath, string agotModPath)
        {
            _submodPath = submodPath;
            _agotModPath = agotModPath;
        }

        public SubmodGeneratorResult Generate(
            LandedTitle county,
            string culture,
            string religion,
            int developmentLevel,
            string englishName,
            string chineseName,
            string referenceCountyKey,
            string titleHistoryRegion,
            List<BaronyInput> baronies)
        {
            var result = new SubmodGeneratorResult();
            try
            {
                string duchyKey = county.Parent != null ? county.Parent.Key : "d_unknown";
                string kingdomKey = (county.Parent != null && county.Parent.Parent != null)
                    ? county.Parent.Parent.Key : "k_unknown";

                AppendProvinceHistory(county.Key, baronies, culture, religion, kingdomKey, result);
                AppendDevelopmentLevel(county.Key, developmentLevel, result);
                AppendTitleHistory(county.Key, duchyKey, referenceCountyKey, titleHistoryRegion, result);
                AppendLocalization(county.Key, englishName, chineseName, baronies, result);

                result.Success = true;
                result.Message = string.Format("成功写入 {0} 个文件", result.GeneratedFiles.Count);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "生成失败: " + ex.Message;
            }
            return result;
        }

        // ── 1. 省份历史 ─────────────────────────────────────────────────

        private void AppendProvinceHistory(string countyKey, List<BaronyInput> baronies,
            string culture, string religion, string kingdomKey, SubmodGeneratorResult result)
        {
            string fileName = string.Format("zzzzzz_{0}_prov.txt", kingdomKey);
            string filePath = Path.Combine(_submodPath, "history", "provinces", fileName);
            EnsureDirectory(filePath);

            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine("# " + countyKey);
            foreach (var b in baronies)
            {
                if (!b.ProvinceId.HasValue) continue;
                sb.AppendLine(b.ProvinceId.Value + " = {");
                sb.AppendLine("\tculture = " + culture);
                sb.AppendLine("\treligion = " + religion);
                sb.AppendLine("\tholding = castle_holding");
                if (!string.IsNullOrWhiteSpace(b.BuildingLevel))
                {
                    sb.AppendLine("\t7824.1.1 = {");
                    sb.AppendLine("\t\tbuildings = { " + b.BuildingLevel + " }");
                    sb.AppendLine("\t}");
                }
                sb.AppendLine("}");
            }
            File.AppendAllText(filePath, sb.ToString(), Encoding.UTF8);
            AddFile(result, filePath);
        }

        // ── 2. 发展度 ───────────────────────────────────────────────────

        private void AppendDevelopmentLevel(string countyKey, int devLevel, SubmodGeneratorResult result)
        {
            string filePath = Path.Combine(_submodPath, "history", "titles", "xsy_z_development_levels.txt");
            EnsureDirectory(filePath);

            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine(countyKey + " = {");
            sb.AppendLine("\t7899.1.1 = {");
            sb.AppendLine("\t\tchange_development_level = " + devLevel);
            sb.AppendLine("\t}");
            sb.AppendLine("}");
            File.AppendAllText(filePath, sb.ToString(), Encoding.UTF8);
            AddFile(result, filePath);
        }

        // ── 3. 统治者历史 ────────────────────────────────────────────────

        private void AppendTitleHistory(string countyKey, string duchyKey,
            string referenceCountyKey, string region, SubmodGeneratorResult result)
        {
            // 从AGOT主mod查找参考伯国的持有者历史
            string refBlock = null;
            if (!string.IsNullOrWhiteSpace(referenceCountyKey))
                refBlock = FindCountyHistoryBlock(referenceCountyKey);

            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine(countyKey + " = {");

            // 初始宗主关系
            sb.AppendLine("\t60.1.1 = {");
            sb.AppendLine("\t\tliege = " + duchyKey);
            sb.AppendLine("\t}");

            // 复制参考伯国的持有者条目
            if (!string.IsNullOrEmpty(refBlock))
            {
                var holderEntries = ExtractHolderEntries(refBlock);
                foreach (var entry in holderEntries)
                {
                    sb.AppendLine(entry);
                }
            }

            sb.AppendLine("}");

            string fileName = string.Format("xsy_titles_{0}.txt", region);
            string filePath = Path.Combine(_submodPath, "history", "titles", fileName);
            EnsureDirectory(filePath);
            File.AppendAllText(filePath, sb.ToString(), Encoding.UTF8);
            AddFile(result, filePath);
        }

        private string FindCountyHistoryBlock(string countyKey)
        {
            string titlesDir = Path.Combine(_agotModPath, "history", "titles");
            if (!Directory.Exists(titlesDir)) return null;

            foreach (var file in Directory.GetFiles(titlesDir, "*.txt"))
            {
                var content = File.ReadAllText(file, Encoding.UTF8);
                var block = ExtractBlock(content, countyKey);
                if (block != null) return block;
            }
            return null;
        }

        private string ExtractBlock(string content, string key)
        {
            // 查找 "key = {" 模式
            string search = key + " = {";
            int idx = content.IndexOf(search, StringComparison.Ordinal);
            if (idx < 0) return null;

            // 检查它不是更长键名的一部分（前面应为换行或空格）
            if (idx > 0)
            {
                char prev = content[idx - 1];
                if (prev != '\n' && prev != '\r' && prev != ' ' && prev != '\t')
                    return null;
            }

            int start = idx + search.Length;
            int depth = 1;
            int pos = start;
            while (pos < content.Length && depth > 0)
            {
                if (content[pos] == '{') depth++;
                else if (content[pos] == '}') depth--;
                pos++;
            }
            return content.Substring(start, pos - start - 1);
        }

        private List<string> ExtractHolderEntries(string blockContent)
        {
            var entries = new List<string>();
            var lines = blockContent.Split('\n');
            bool inEntry = false;
            bool hasHolder = false;
            var entryLines = new List<string>();
            int depth = 0;

            foreach (var rawLine in lines)
            {
                string line = rawLine.TrimEnd('\r');
                int open = CountChar(line, '{');
                int close = CountChar(line, '}');

                if (!inEntry && depth == 0)
                {
                    string trimmed = line.Trim();
                    // 日期条目: 以数字开头，且包含 "= {"
                    if (trimmed.Length > 0 && char.IsDigit(trimmed[0]) && trimmed.Contains("= {"))
                    {
                        inEntry = true;
                        hasHolder = false;
                        entryLines.Clear();
                        entryLines.Add(line);
                        depth += open - close;
                        continue;
                    }
                }

                if (inEntry)
                {
                    entryLines.Add(line);
                    depth += open - close;
                    if (line.IndexOf("holder", StringComparison.OrdinalIgnoreCase) >= 0 &&
                        line.Contains("="))
                        hasHolder = true;

                    if (depth <= 0)
                    {
                        inEntry = false;
                        depth = 0;
                        if (hasHolder)
                            entries.Add(string.Join("\n", entryLines));
                    }
                }
            }
            return entries;
        }

        private static int CountChar(string s, char c)
        {
            int count = 0;
            foreach (char ch in s)
                if (ch == c) count++;
            return count;
        }

        // ── 4. 本地化 ────────────────────────────────────────────────────

        private void AppendLocalization(string countyKey, string englishName, string chineseName,
            List<BaronyInput> baronies, SubmodGeneratorResult result)
        {
            string enPath = Path.Combine(_submodPath, "localization", "english", "xsy_titles_l_english.yml");
            string cnPath = Path.Combine(_submodPath, "localization", "simp_chinese", "xsy_titles_l_simp_chinese.yml");

            AppendLocFile(enPath, "l_english", countyKey, englishName, baronies, true, result);
            AppendLocFile(cnPath, "l_simp_chinese", countyKey, chineseName, baronies, false, result);
        }

        private void AppendLocFile(string filePath, string langHeader, string countyKey,
            string countyName, List<BaronyInput> baronies, bool isEnglish, SubmodGeneratorResult result)
        {
            EnsureDirectory(filePath);

            var sb = new StringBuilder();

            // 文件不存在时写入头部
            if (!File.Exists(filePath))
                sb.AppendLine(langHeader + ":");

            if (!string.IsNullOrEmpty(countyName))
                sb.AppendLine(" " + countyKey + ":0 \"" + countyName + "\"");

            foreach (var b in baronies)
            {
                string bName = isEnglish ? b.EnglishName : b.ChineseName;
                if (!string.IsNullOrEmpty(bName))
                    sb.AppendLine(" " + b.Key + ":0 \"" + bName + "\"");
            }

            // CK3本地化文件需要UTF-8 BOM
            File.AppendAllText(filePath, sb.ToString(), new UTF8Encoding(true));
            AddFile(result, filePath);
        }

        // ── 从主mod提取文化/宗教列表 ────────────────────────────────────

        /// <summary>
        /// 扫描AGOT主mod的省份历史文件，提取所有出现的文化和宗教标识符。
        /// </summary>
        public static void ExtractCulturesAndReligions(string agotModPath,
            out List<string> cultures, out List<string> religions)
        {
            var cultSet = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
            var relSet = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);

            string provDir = Path.Combine(agotModPath, "history", "provinces");
            if (Directory.Exists(provDir))
            {
                foreach (var file in Directory.GetFiles(provDir, "*.txt"))
                {
                    string[] lines;
                    try { lines = File.ReadAllLines(file, Encoding.UTF8); }
                    catch { continue; }

                    foreach (var rawLine in lines)
                    {
                        string line = rawLine.Trim();
                        int hash = line.IndexOf('#');
                        if (hash >= 0) line = line.Substring(0, hash).Trim();

                        if (line.StartsWith("culture =", StringComparison.Ordinal))
                        {
                            string val = line.Substring("culture =".Length).Trim();
                            if (!string.IsNullOrEmpty(val)) cultSet.Add(val);
                        }
                        else if (line.StartsWith("religion =", StringComparison.Ordinal))
                        {
                            string val = line.Substring("religion =".Length).Trim();
                            if (!string.IsNullOrEmpty(val)) relSet.Add(val);
                        }
                    }
                }
            }

            cultures = new List<string>(cultSet);
            religions = new List<string>(relSet);
        }

        /// <summary>
        /// 从AGOT主mod的省份历史文件中查找指定省份ID的文化和宗教。
        /// </summary>
        public static void GetProvinceInfo(string agotModPath, int provinceId,
            out string culture, out string religion)
        {
            culture = null;
            religion = null;

            string provDir = Path.Combine(agotModPath, "history", "provinces");
            if (!Directory.Exists(provDir)) return;

            string idStr = provinceId.ToString();
            foreach (var file in Directory.GetFiles(provDir, "*.txt"))
            {
                string[] lines;
                try { lines = File.ReadAllLines(file, Encoding.UTF8); }
                catch { continue; }

                bool inBlock = false;
                int depth = 0;

                foreach (var rawLine in lines)
                {
                    string line = rawLine;
                    int hash = line.IndexOf('#');
                    if (hash >= 0) line = line.Substring(0, hash);
                    string trimmed = line.Trim();

                    if (!inBlock)
                    {
                        // 匹配 "PROVID = {"
                        if (trimmed == idStr + " = {" ||
                            trimmed.StartsWith(idStr + " = {", StringComparison.Ordinal))
                        {
                            inBlock = true;
                            depth = CountChar(line, '{') - CountChar(line, '}');
                            continue;
                        }
                    }
                    else
                    {
                        depth += CountChar(line, '{') - CountChar(line, '}');

                        if (trimmed.StartsWith("culture =", StringComparison.Ordinal) && depth == 1)
                        {
                            string val = trimmed.Substring("culture =".Length).Trim();
                            if (!string.IsNullOrEmpty(val)) culture = val;
                        }
                        else if (trimmed.StartsWith("religion =", StringComparison.Ordinal) && depth == 1)
                        {
                            string val = trimmed.Substring("religion =".Length).Trim();
                            if (!string.IsNullOrEmpty(val)) religion = val;
                        }

                        if (depth <= 0)
                        {
                            if (culture != null || religion != null) return;
                            inBlock = false;
                        }
                    }
                }

                if (culture != null || religion != null) return;
            }
        }

        // ── 工具方法 ─────────────────────────────────────────────────────

        private static void EnsureDirectory(string filePath)
        {
            string dir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);
        }

        private static void AddFile(SubmodGeneratorResult result, string filePath)
        {
            if (!result.GeneratedFiles.Contains(filePath))
                result.GeneratedFiles.Add(filePath);
        }
    }
}
