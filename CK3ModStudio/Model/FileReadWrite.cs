using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Xml.Linq;
using HandyControl.Controls;
using Prism.Mvvm;
using WpfPrismFrameworkTemplate.Helper;
using WpfPrismFrameworkTemplate.Views;
using System.Windows.Media;

namespace WpfPrismFrameworkTemplate.Model
{
    public class FileReadWrite : BindableBase
    {
        private string _dynastyDefFile;
        private string _characterDefFile;
        private string _domainDefFile;
        private string _coaDefFile;
        private string _localizationEnglishFile;
        private string _localizationChineseFile;
        private string _CoaCollectSoftwarePath;
        private string _CoaSavePath;
        private string _CoaDefinePath;
        private string _RandomNameFilePath;
        private bool _EnableAutoSave=true;
        private bool _EnableRandomAge = true;
        private bool _IsShowDebugInfo = false;

        public FileReadWrite()
        {

        }

        [Category("定义文件")]
        [DisplayName("王朝定义文件")]
        [Description("选择或输入王朝定义文件的路径")]
        [Editor(typeof(FilePathSelectorEditor), typeof(FilePathSelectorEditor))]
        public string DynastyDefFile
        {
            get => _dynastyDefFile;
            set => SetProperty(ref _dynastyDefFile, value);
        }

        [Category("定义文件")]
        [DisplayName("角色定义文件")]
        [Description("选择或输入角色定义文件的路径")]
        [Editor(typeof(FilePathSelectorEditor), typeof(FilePathSelectorEditor))]
        public string CharacterDefFile
        {
            get => _characterDefFile;
            set => SetProperty(ref _characterDefFile, value);
        }

        [Category("定义文件")]
        [DisplayName("领地定义文件")]
        [Description("选择或输入领地定义文件的路径")]
        [Editor(typeof(FilePathSelectorEditor), typeof(FilePathSelectorEditor))]
        public string DomainDefFile
        {
            get => _domainDefFile;
            set => SetProperty(ref _domainDefFile, value);
        }

        [Category("本地化文件")]
        [DisplayName("英语翻译文件")]
        [Description("选择或输入英语本地化翻译文件的路径")]
        [Editor(typeof(FilePathSelectorEditor), typeof(FilePathSelectorEditor))]
        public string LocalizationEnglishFile
        {
            get => _localizationEnglishFile;
            set => SetProperty(ref _localizationEnglishFile, value);
        }

        [Category("本地化文件")]
        [DisplayName("汉语翻译文件")]
        [Description("选择或输入汉语本地化翻译文件的路径")]
        [Editor(typeof(FilePathSelectorEditor), typeof(FilePathSelectorEditor))]
        public string LocalizationChineseFile
        {
            get => _localizationChineseFile;
            set => SetProperty(ref _localizationChineseFile, value);
        }

        [Category("保存设置")]
        [DisplayName("是否允许自动保存")]
        [Description("是否允许关闭软件后自动保存")]
        public bool EnableAutoSave
        {
            get => _EnableAutoSave;
            set => SetProperty(ref _EnableAutoSave, value);
        }

        [Category("纹章收集设置")]
        [DisplayName("纹章收集软件启动路径")]
        [Description("选择纹章收集软件启动路径")]
        [Editor(typeof(FilePathSelectorEditor), typeof(FilePathSelectorEditor))]
        public string CoaCollectSoftwarePath
        {
            get => _CoaCollectSoftwarePath;
            set => SetProperty(ref _CoaCollectSoftwarePath, value);
        }

        [Category("纹章收集设置")]
        [DisplayName("纹章图片保存路径")]
        [Description("选择纹章图片保存路径")]
        [Editor(typeof(FilePathSelectorEditor), typeof(FilePathSelectorEditor))]
        public string CoaSavePath
        {
            get => _CoaSavePath;
            set => SetProperty(ref _CoaSavePath, value);
        }

        [Category("纹章收集设置")]
        [DisplayName("纹章定义文件路径")]
        [Description("选择纹章定义文件路径")]
        [Editor(typeof(FilePathSelectorEditor), typeof(FilePathSelectorEditor))]
        public string CoaDefinePath
        {
            get => _CoaDefinePath;
            set => SetProperty(ref _CoaDefinePath, value);
        }

        [Category("随机设置")]
        [DisplayName("随机名字/姓氏文件路径")]
        [Description("选择随机名字/姓氏文件")]
        [Editor(typeof(FilePathSelectorEditor), typeof(FilePathSelectorEditor))]
        public string RandomNameFilePath
        {
            get => _RandomNameFilePath;
            set => SetProperty(ref _RandomNameFilePath, value);
        }

        [Category("随机设置")]
        [DisplayName("是否开启随机年龄")]
        [Description("选择是否开启随机年龄")]
        public bool EnableRandomAge
        {
            get => _EnableRandomAge;
            set => SetProperty(ref _EnableRandomAge, value);
        }

        [Category("调试设置")]
        [DisplayName("是否显示调试信息")]
        [Description("是否显示调试信息")]
        public bool IsShowDebugInfo
        {
            get => _IsShowDebugInfo;
            set => SetProperty(ref _IsShowDebugInfo, value);
        }
    }
}
