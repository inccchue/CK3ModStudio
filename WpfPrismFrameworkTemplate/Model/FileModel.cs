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
        public abstract void Add(ObservableCollection<Family> familyList);

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

                // 移除最后的空白行（如果存在）
                text = text.TrimEnd('\r', '\n');

                // 写入文件
                File.WriteAllText(FilePath, text);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存文件出错: {ex.Message}");
            }
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

        public override void Add(ObservableCollection<Family> familyList)
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

        public override void Add(ObservableCollection<Family> familyList)
        {
            if (familyList == null || familyList.Count == 0)
            {
                return;
            }
            // 过滤出 IdName 不在 Characters 字典中 key 的 People 对象
            ObservableCollection<People> peoples = new ObservableCollection<People>();
            foreach (Family family in familyList)
            {
                peoples.AddRange(family.Members);
            }
            var filteredPeople = peoples
                .Where(p => !_characters.ContainsKey(p.IdName))
                .ToList();

            string newContent = "";
            foreach (var people in filteredPeople)
            {
                newContent += people.GetString();
                _characters[people.IdName] = newContent;
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

        public override void Add(ObservableCollection<Family> familyList)
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

        public override void Add(ObservableCollection<Family> familyList)
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

        public override void Add(ObservableCollection<Family> familyList)
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

        public override void Add(ObservableCollection<Family> familyList)
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
