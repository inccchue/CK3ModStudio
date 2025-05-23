using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;
using System.Windows.Documents;
using System.IO;
using System.Windows.Media;
using System.Windows;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using Unity.Injection;
using MySql.Data.MySqlClient;
using System.Collections.ObjectModel;
using System.Windows.Media.TextFormatting;
using Unity;

namespace WpfPrismFrameworkTemplate.Model
{
    /// <summary>
    /// 文件基类，定义公共属性和方法
    /// </summary>
    public abstract class FileModel : BindableBase
    {
        private string _filePath;
        private FlowDocument _content;

        public string FilePath
        {
            get => _filePath;
            set => SetProperty(ref _filePath, value);
        }

        public FlowDocument Content
        {
            get => _content;
            set => SetProperty(ref _content, value);
        }

        protected FileModel()
        {
            _content = new FlowDocument();
        }

        public virtual void Load()
        {
            // 添加黑色文本段落
            Paragraph paragraph = new Paragraph();
            try
            {
                if (string.IsNullOrEmpty(FilePath) || !File.Exists(FilePath))
                {
                    return;
                }

                string content = File.ReadAllText(FilePath); 


                Run run = new Run(content);
                run.Foreground = Brushes.Black;
                paragraph.Inlines.Add(run);
                Content.Blocks.Clear();
                Content.Blocks.Add(paragraph);
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载文件出错: {ex.Message}");
                return;
            }
        }

        /// <summary>
        /// 添加新内容的抽象方法，由子类实现
        /// </summary>
        /// <param name="newContent">要添加的新内容</param>
        public abstract void UpdateAllContent(ObservableCollection<Family> familyList);

        public void Save()
        {
            try
            {
                if (string.IsNullOrEmpty(FilePath))
                {
                    return;
                }

                // 提取纯文本
                TextRange textRange = new TextRange(Content.ContentStart, Content.ContentEnd);
                string text = textRange.Text;
                string[] lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                string result = string.Join(Environment.NewLine, lines.Where(line => !string.IsNullOrWhiteSpace(line)));

                // 写入文件
                File.WriteAllText(FilePath, result);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存文件出错: {ex.Message}");
            }
        }
        protected void RemoveContentInBlocks(string delContent)
        {
            foreach (Block block in Content.Blocks)
            {
                if (block is Paragraph paragraph)
                {
                    string fullText = "";
                    List<Run> originalRuns = new List<Run>();

                    foreach (Inline inline in paragraph.Inlines)
                    {
                        if (inline is Run run)
                        {
                            fullText += run.Text;
                            originalRuns.Add(run);
                        }
                    }

                    if (fullText.Contains(delContent))
                    {
                        int index = fullText.IndexOf(delContent);
                        string beforeText = fullText.Substring(0, index);
                        string afterText = fullText.Substring(index + delContent.Length);

                        paragraph.Inlines.Clear();

                        // 添加删除前的内容
                        if (!string.IsNullOrEmpty(beforeText))
                        {
                            Run beforeRun = new Run(beforeText);
                            if (originalRuns.Count > 0 && originalRuns.All(r => r.Foreground?.ToString() == originalRuns[0].Foreground?.ToString()))
                            {
                                beforeRun.Foreground = originalRuns[0].Foreground;
                            }
                            paragraph.Inlines.Add(beforeRun);
                        }

                        // 不添加delContent，相当于删除它

                        // 添加删除后的内容
                        if (!string.IsNullOrEmpty(afterText))
                        {
                            Run afterRun = new Run(afterText);
                            if (originalRuns.Count > 0 && originalRuns.All(r => r.Foreground?.ToString() == originalRuns[0].Foreground?.ToString()))
                            {
                                afterRun.Foreground = originalRuns[0].Foreground;
                            }
                            paragraph.Inlines.Add(afterRun);
                        }

                        return; // 删除后直接返回
                    }
                }
            }
        }

        protected void ReplaceContentInBlocks(string oldContent, string newContent)
        {
            foreach (Block block in Content.Blocks)
            {
                if (block is Paragraph paragraph)
                {
                    // 将所有Run的文本连接起来，重新构建完整文本
                    string fullText = "";
                    List<Run> originalRuns = new List<Run>();

                    foreach (Inline inline in paragraph.Inlines)
                    {
                        if (inline is Run run)
                        {
                            fullText += run.Text;
                            originalRuns.Add(run);
                        }
                    }

                    // 检查完整文本是否包含要替换的内容
                    if (fullText.Contains(oldContent))
                    {
                        // 找到oldContent在完整文本中的位置
                        int index = fullText.IndexOf(oldContent);

                        // 清除原有的所有Run
                        paragraph.Inlines.Clear();

                        // 分割文本
                        string beforeText = fullText.Substring(0, index);
                        string afterText = fullText.Substring(index + oldContent.Length);

                        // 添加oldContent之前的文本
                        if (!string.IsNullOrEmpty(beforeText))
                        {
                            Run beforeRun = new Run(beforeText);
                            // 保持原有颜色（如果所有原Run颜色相同的话）
                            if (originalRuns.Count > 0 && originalRuns.All(r => r.Foreground?.ToString() == originalRuns[0].Foreground?.ToString()))
                            {
                                beforeRun.Foreground = originalRuns[0].Foreground;
                            }
                            paragraph.Inlines.Add(beforeRun);
                        }

                        // 添加替换后的内容，只对不同的部分设置特殊样式
                        AddDifferenceHighlightedRuns(paragraph, oldContent, newContent);

                        // 添加oldContent之后的文本
                        if (!string.IsNullOrEmpty(afterText))
                        {
                            Run afterRun = new Run(afterText);
                            // 保持原有颜色
                            if (originalRuns.Count > 0 && originalRuns.All(r => r.Foreground?.ToString() == originalRuns[0].Foreground?.ToString()))
                            {
                                afterRun.Foreground = originalRuns[0].Foreground;
                            }
                            paragraph.Inlines.Add(afterRun);
                        }

                        return; // 找到并替换后直接返回
                    }
                }
            }
        }

        protected void AddDifferenceHighlightedRuns(Paragraph paragraph, string oldContent, string newContent)
        {
            // 使用简单的字符比较来找出差异
            var oldLines = oldContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var newLines = newContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            int maxLines = Math.Max(oldLines.Length, newLines.Length);

            for (int i = 0; i < maxLines; i++)
            {
                string oldLine = i < oldLines.Length ? oldLines[i] : "";
                string newLine = i < newLines.Length ? newLines[i] : "";

                if (oldLine == newLine)
                {
                    // 相同的行，保持原样式
                    if (!string.IsNullOrEmpty(newLine))
                    {
                        paragraph.Inlines.Add(new Run(newLine));
                        if (i < maxLines - 1) // 不是最后一行时添加换行
                        {
                            paragraph.Inlines.Add(new Run("\r\n"));
                        }
                    }
                }
                else
                {
                    // 不同的行，查找具体差异
                    var diffRuns = GetDifferenceRuns(oldLine, newLine);
                    foreach (var diffRun in diffRuns)
                    {
                        paragraph.Inlines.Add(diffRun);
                    }
                    if (i < maxLines - 1) // 不是最后一行时添加换行
                    {
                        paragraph.Inlines.Add(new Run("\r\n"));
                    }
                }
            }
        }

        protected List<Run> GetDifferenceRuns(string oldLine, string newLine)
        {
            List<Run> runs = new List<Run>();

            // 简单的字符级别比较
            int i = 0, j = 0;
            string commonStart = "";

            // 找到开头相同的部分
            while (i < oldLine.Length && j < newLine.Length && oldLine[i] == newLine[j])
            {
                commonStart += oldLine[i];
                i++;
                j++;
            }

            // 添加相同的开头部分
            if (!string.IsNullOrEmpty(commonStart))
            {
                runs.Add(new Run(commonStart));
            }

            // 找到结尾相同的部分
            string commonEnd = "";
            int oldEnd = oldLine.Length - 1;
            int newEnd = newLine.Length - 1;

            while (oldEnd >= i && newEnd >= j && oldLine[oldEnd] == newLine[newEnd])
            {
                commonEnd = oldLine[oldEnd] + commonEnd;
                oldEnd--;
                newEnd--;
            }

            // 添加中间不同的部分
            string differentPart = newLine.Substring(j, newEnd - j + 1);
            if (!string.IsNullOrEmpty(differentPart))
            {
                Run differentRun = new Run(differentPart);
                differentRun.Foreground = Brushes.Green;
                differentRun.TextDecorations = TextDecorations.Underline;
                runs.Add(differentRun);
            }

            // 添加相同的结尾部分
            if (!string.IsNullOrEmpty(commonEnd))
            {
                runs.Add(new Run(commonEnd));
            }

            return runs;
        }
    }

    /// <summary>
    /// 王朝定义文件模型
    /// </summary>
    public class DynastyDefFileModel : FileModel
    {
        // 存储解析出来的家族ID和家族定义的字典
        protected Dictionary<string, string> _dynasties = new Dictionary<string, string>();

        
        public DynastyDefFileModel() : base()
        {
        }

        public override void Load()
        {
            base.Load();
            ParseDynastyDefinitions();
        }
        public void RemoveSingleFamily(Family targetFamily, string targetContent)
        {
            if (targetFamily == null)
            {
                return;
            }

            _dynasties.Remove(targetFamily.FamilyName);
            string currentContent = targetContent;
            RemoveContentInBlocks(currentContent);


        }
        public void UpdateSingleFamily(Family targetFamily,string targetContent)
        {
            if (targetFamily == null)
            {
                return;
            }
            string currentContent = targetContent;

            if (!_dynasties.ContainsKey(targetFamily.FamilyName))
            {
                // 新的People对象
                _dynasties[targetFamily.FamilyName] = currentContent;

                Paragraph newParagraph = new Paragraph();
                Run newRun = new Run(currentContent);
                newRun.Foreground = Brushes.Red;
                newParagraph.Inlines.Add(newRun);
                Content.Blocks.Add(newParagraph);
            }
            else
            {
                string oldContent = _dynasties[targetFamily.FamilyName];
                if (oldContent != currentContent)
                {
                    _dynasties[targetFamily.FamilyName] = currentContent;
                    ReplaceContentInBlocks(oldContent, currentContent);
                }
            }
        }

        public override void UpdateAllContent(ObservableCollection<Family> familyList)
        {
            if (familyList == null || familyList.Count == 0)
            {
                return;
            }

            var filteredFamily = familyList
                .Where(f => !_dynasties.ContainsKey(f.FamilyName))
                .ToList();

            string newContent = "";
            foreach (var family in filteredFamily)
            {
                newContent += family.GetString();
                _dynasties[family.FamilyName] = newContent;
                newContent += "\r\n";              
            }

            // 添加红色文本段落
            Paragraph paragraph = new Paragraph();
            Run run = new Run(newContent);
            run.Foreground = Brushes.Red;
            paragraph.Inlines.Add(run);
            Content.Blocks.Add(paragraph);
        }

        /// <summary>
        /// 解析家族定义字符串，提取家族名和定义
        /// </summary>
        /// <param name="content">包含家族定义的内容</param>
        public virtual void ParseDynastyDefinitions()
        {
            TextRange textRange = new TextRange(Content.ContentStart, Content.ContentEnd);
            // 使用正则表达式查找家族定义
            // 格式： dynn_Name = { ... }
            Regex dynastyPattern = new Regex(@"(dynn_\w+)\s*=\s*\{([^{}]*(?:\{[^{}]*\}[^{}]*)*)\}", RegexOptions.Singleline);

            MatchCollection matches = dynastyPattern.Matches(textRange.Text);
            foreach (Match match in matches)
            {
                if (match.Groups.Count >= 3)
                {
                    // 获取带前缀的ID
                    string dynastyIdWithPrefix = match.Groups[1].Value.Trim();

                    // 去掉前缀，获取实际的家族名
                    string dynastyName = RemovePrefix(dynastyIdWithPrefix);

                    // 保存完整定义
                    string definition = match.Value.Trim();
                    _dynasties[dynastyName] = definition;
                }
            }
        }

        /// <summary>
        /// 去掉家族名前缀
        /// </summary>
        /// <param name="dynastyIdWithPrefix">带前缀的家族ID</param>
        /// <returns>不带前缀的家族名</returns>
        protected string RemovePrefix(string dynastyIdWithPrefix)
        {
            if (dynastyIdWithPrefix.StartsWith(Family.DYNASTY_PREFIX))
            {
                return dynastyIdWithPrefix.Substring(Family.DYNASTY_PREFIX.Length);
            }
            return dynastyIdWithPrefix;
        }

        /// <summary>
        /// 添加前缀到家族名
        /// </summary>
        /// <param name="dynastyName">不带前缀的家族名</param>
        /// <returns>带前缀的家族ID</returns>
        private string AddPrefix(string dynastyName)
        {
            if (!dynastyName.StartsWith(Family.DYNASTY_PREFIX))
            {
                return Family.DYNASTY_PREFIX + dynastyName;
            }
            return dynastyName;
        }
    }

    /// <summary>
    /// 角色定义文件模型
    /// </summary>
    public class CharacterDefFileModel : FileModel
    {
        private Dictionary<string, string> _characters = new Dictionary<string, string>();
        public CharacterDefFileModel():base()
        {
        }

        public override void Load()
        {
            base.Load();
            ParseCharacterDefinitions();
        }
        public void RemoveSingleCharacter(People targetPeople)
        {
            if (targetPeople == null)
            {
                return;
            }          

            _characters.Remove(targetPeople.IdName);
            string currentContent = targetPeople.GetString();
            RemoveContentInBlocks(currentContent);


        }
        public void UpdateSingleCharacter(People targetPeople)
        {
            if (targetPeople == null)
            {
                return;
            }
            string currentContent = targetPeople.GetString();

            if (!_characters.ContainsKey(targetPeople.IdName))
            {
                // 新的People对象
                _characters[targetPeople.IdName] = currentContent;

                Paragraph newParagraph = new Paragraph();
                Run newRun = new Run(currentContent);
                newRun.Foreground = Brushes.Red;
                newParagraph.Inlines.Add(newRun);
                Content.Blocks.Add(newParagraph);
            }
            else
            {
                // 检查现有People对象内容是否发生变化
                string oldContent = _characters[targetPeople.IdName];
                if (oldContent != currentContent)
                {
                    _characters[targetPeople.IdName] = currentContent;
                    ReplaceContentInBlocks(oldContent, currentContent);
                }
            }

                      
        }
        public override void UpdateAllContent(ObservableCollection<Family> familyList)
        {
            if (familyList == null || familyList.Count == 0)
            {
                return;
            }

            // 获取所有People对象
            ObservableCollection<People> peoples = new ObservableCollection<People>();
            foreach (Family family in familyList)
            {
                peoples.AddRange(family.Members);
            }

            string newContent = "";
            List<(string oldContent, string newContent)> updatedContents = new List<(string, string)>();

            foreach (var people in peoples)
            {
                string currentContent = people.GetString();

                if (!_characters.ContainsKey(people.IdName))
                {
                    // 新的People对象
                    _characters[people.IdName] = currentContent;
                    newContent += currentContent + "\r\n";
                }
                else
                {
                    // 检查现有People对象内容是否发生变化
                    string oldContent = _characters[people.IdName];
                    if (oldContent != currentContent)
                    {
                        // 内容发生了变化，记录旧内容和新内容
                        updatedContents.Add((oldContent, currentContent));
                        _characters[people.IdName] = currentContent;
                    }
                }
            }

            // 处理内容更新 - 在Content中查找并替换
            foreach (var (oldContent, updatedContent) in updatedContents)
            {
                ReplaceContentInBlocks(oldContent, updatedContent);
            }

            // 添加新内容（红色文本）
            if (!string.IsNullOrEmpty(newContent))
            {
                Paragraph newParagraph = new Paragraph();
                Run newRun = new Run(newContent.TrimEnd('\r', '\n'));
                newRun.Foreground = Brushes.Red;
                newParagraph.Inlines.Add(newRun);
                Content.Blocks.Add(newParagraph);
            }
        }

        

        /// <summary>
        /// 解析角色定义字符串，提取角色ID和定义
        /// </summary>
        /// <param name="content">包含角色定义的内容</param>
        public void ParseCharacterDefinitions()
        {
            TextRange textRange = new TextRange(Content.ContentStart, Content.ContentEnd);
            // 使用正则表达式查找角色定义
            // 格式： CharacterId = { ... }
            Regex characterPattern = new Regex(@"(\w+_\d+)\s*=\s*\{([^{}]*(?:\{[^{}]*\}[^{}]*)*)\}", RegexOptions.Singleline);

            MatchCollection matches = characterPattern.Matches(textRange.Text);
            foreach (Match match in matches)
            {
                if (match.Groups.Count >= 3)
                {
                    string characterId = match.Groups[1].Value.Trim();
                    string definition = match.Value.Trim();
                    _characters[characterId] = definition;
                }
            }
        }
    }

    /// <summary>
    /// 领域定义文件模型
    /// </summary>
    public class DomainDefFileModel : FileModel
    {
        public DomainDefFileModel() : base()
        {
        }

        public override void UpdateAllContent(ObservableCollection<Family> familyList)
        {

        }
    }

    /// <summary>
    /// 纹章定义文件模型
    /// </summary>
    public class CoaDefFileModel : FileModel
    {
        public CoaDefFileModel() : base()
        {
        }

        public override void UpdateAllContent(ObservableCollection<Family> familyList)
        {

        }
    }

    /// <summary>
    /// 英文本地化文件模型
    /// </summary>
    public class LocalizationEnglishFileModel : DynastyDefFileModel
    {
        public LocalizationEnglishFileModel() : base()
        {
        }
       
        public override void UpdateAllContent(ObservableCollection<Family> familyList)
        {
            if (familyList == null || familyList.Count == 0)
            {
                return;
            }
            var filteredFamily = familyList
                .Where(f => !_dynasties.ContainsKey(f.FamilyName))
                .ToList();

            string newContent = "";
            foreach (var family in filteredFamily)
            {
                newContent += family.GetLocalizationString();
                _dynasties[family.FamilyName] = newContent;
            }

            // 添加红色文本段落
            Paragraph paragraph = new Paragraph();
            Run run = new Run(newContent);
            run.Foreground = Brushes.Red;
            paragraph.Inlines.Add(run);
            Content.Blocks.Add(paragraph);
        }

        /// <summary>
        /// 解析本地化文件内容，提取条目到字典中
        /// </summary>
        /// <param name="content">本地化文件内容</param>
        public override void ParseDynastyDefinitions()
        {
            TextRange textRange = new TextRange(Content.ContentStart, Content.ContentEnd);
            // 首先检查是否有"l_english:"头部
            bool hasHeader = textRange.Text.Contains("l_english:");

            // 使用正则表达式查找本地化条目
            // 格式: key:0 "value"
            Regex localizationPattern = new Regex(@"(\w+):0\s+""([^""]*)""", RegexOptions.Multiline);

            MatchCollection matches = localizationPattern.Matches(textRange.Text);
            foreach (Match match in matches)
            {
                if (match.Groups.Count >= 3)
                {
                    string dynastyIdWithPrefix = match.Groups[1].Value.Trim();

                    // 去掉前缀，获取实际的家族名
                    string dynastyName = RemovePrefix(dynastyIdWithPrefix);
                    string value = match.Groups[2].Value.Trim();
                    _dynasties[dynastyName] = value;
                }
            }
        }
    }

    /// <summary>
    /// 中文本地化文件模型
    /// </summary>
    public class LocalizationChineseFileModel : LocalizationEnglishFileModel
    {
        public LocalizationChineseFileModel() : base()
        {
        }

        public override void UpdateAllContent(ObservableCollection<Family> familyList)
        {
            if (familyList == null || familyList.Count == 0)
            {
                return;
            }
            var filteredFamily = familyList
                .Where(f => !_dynasties.ContainsKey(f.FamilyName))
                .ToList();

            string newContent = "";
            foreach (var family in filteredFamily)
            {
                newContent += family.GetLocalizationString_CN();
                _dynasties[family.FamilyName] = newContent;
            }

            // 添加红色文本段落
            Paragraph paragraph = new Paragraph();
            Run run = new Run(newContent);
            run.Foreground = Brushes.Red;
            paragraph.Inlines.Add(run);
            Content.Blocks.Add(paragraph);
        }
    }
}
