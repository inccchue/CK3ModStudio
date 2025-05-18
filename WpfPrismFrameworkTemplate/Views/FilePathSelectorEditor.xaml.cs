using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using Xceed.Wpf.Toolkit.PropertyGrid;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;

namespace WpfPrismFrameworkTemplate.Views
{
    /// <summary>
    /// FilePathSelectorEditor.xaml 的交互逻辑
    /// </summary>
    public partial class FilePathSelectorEditor : UserControl, ITypeEditor
    {
        public FilePathSelectorEditor()
        {
            InitializeComponent();
        }
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            "Value",
            typeof(string),
            typeof(FilePathSelectorEditor),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public string Value
        {
            get { return (string)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            // 默认的过滤器和标题
            string filter = "All files (*.*)|*.*";
            string title = "Select a file";

            // 获取属性项
            var propertyItem = BindingOperations.GetBindingExpression(this, ValueProperty).DataItem as PropertyItem;

            // 根据属性名称设置不同的过滤器和标题
            if (propertyItem != null)
            {
                string propertyName = propertyItem.PropertyName;

                if (propertyName.Contains("Dynasty"))
                {
                    filter = "Dynasty files (*.txt;*.xml)|*.txt;*.xml|All files (*.*)|*.*";
                    title = "Select Dynasty Definition File";
                }
                else if (propertyName.Contains("Character"))
                {
                    filter = "Character files (*.txt;*.xml)|*.txt;*.xml|All files (*.*)|*.*";
                    title = "Select Character Definition File";
                }
                else if (propertyName.Contains("Domain"))
                {
                    filter = "Domain files (*.txt;*.xml)|*.txt;*.xml|All files (*.*)|*.*";
                    title = "Select Domain Definition File";
                }
                else if (propertyName.Contains("CoaDef"))
                {
                    filter = "CoA files (*.txt;*.xml)|*.txt;*.xml|All files (*.*)|*.*";
                    title = "Select CoA Definition File";
                }
                else if (propertyName.Contains("English"))
                {
                    filter = "Localization files (*.yml;*.xml)|*.yml;*.xml|All files (*.*)|*.*";
                    title = "Select English Localization File";
                }
                else if (propertyName.Contains("Chinese"))
                {
                    filter = "Localization files (*.yml;*.xml)|*.yml;*.xml|All files (*.*)|*.*";
                    title = "Select Chinese Localization File";
                }
                else if (propertyName.Contains("CoaCollect"))
                {
                    filter = "Localization files (*.exe)|*.exe|All files (*.*)|*.*";
                    title = "Select Chinese Localization File";
                }
                else if (propertyName.Contains("Random"))
                {
                    filter = "Character files (*.txt;*.xml)|*.txt;*.xml|All files (*.*)|*.*";
                    title = "Select Character Definition File";
                }
            }

            var dialog = new OpenFileDialog
            {
                Filter = filter,
                Title = title
            };

            // 如果已有文件路径，设置初始目录
            if (!string.IsNullOrEmpty(Value))
            {
                try
                {
                    dialog.InitialDirectory = System.IO.Path.GetDirectoryName(Value);
                    dialog.FileName = System.IO.Path.GetFileName(Value);
                }
                catch { }
            }

            if (dialog.ShowDialog() == true)
            {
                Value = dialog.FileName;
            }
        }

        public FrameworkElement ResolveEditor(PropertyItem propertyItem)
        {
            Binding binding = new Binding("Value");
            binding.Source = propertyItem;
            binding.Mode = propertyItem.IsReadOnly ? BindingMode.OneWay : BindingMode.TwoWay;
            BindingOperations.SetBinding(this, FilePathSelectorEditor.ValueProperty, binding);
            return this;
        }
    }
}
