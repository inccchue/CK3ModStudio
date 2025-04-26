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
        private bool _EnableAutoSave=true;

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

        [Category("定义文件")]
        [DisplayName("纹章定义文件")]
        [Description("选择或输入纹章(CoA)定义文件的路径")]
        [Editor(typeof(FilePathSelectorEditor), typeof(FilePathSelectorEditor))]
        public string CoaDefFile
        {
            get => _coaDefFile;
            set => SetProperty(ref _coaDefFile, value);
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
    }
}
