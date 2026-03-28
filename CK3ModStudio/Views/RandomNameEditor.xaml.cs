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
using WpfPrismFrameworkTemplate.Helper;
using WpfPrismFrameworkTemplate.Model;
using Xceed.Wpf.Toolkit.PropertyGrid;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;

namespace WpfPrismFrameworkTemplate.Views
{
    /// <summary>
    /// RandomNameEditor.xaml 的交互逻辑
    /// </summary>
    public partial class RandomNameEditor : UserControl, ITypeEditor
    {
        private Random _random = new Random();

        // 值依赖属性（用于绑定到PropertyGrid的Value）
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            "Value",
            typeof(People),
            typeof(RandomNameEditor),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        // 名字源依赖属性（用于绑定随机名字列表）
        public static readonly DependencyProperty NameSourceProperty = DependencyProperty.Register(
            "NameSource",
            typeof(List<CultureNames>),
            typeof(RandomNameEditor),
            new PropertyMetadata(null));

        public People Value
        {
            get { return (People)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public List<CultureNames> NameSource
        {
            get { return (List<CultureNames>)GetValue(NameSourceProperty); }
            set { SetValue(NameSourceProperty, value); }
        }

        public RandomNameEditor()
        {
            InitializeComponent();
        }

        private void RandomButton_Click(object sender, RoutedEventArgs e)
        {
            GenerateRandomName();
        }

        private void GenerateRandomName()
        {
            // 获取随机名字源
            CultureNames names = GetNameSource();

            if (names != null)
            {
                if(Value.Gender == GenderType.Male && names.MaleNames.Count() > 0)
                {
                    int index = _random.Next(0, names.MaleNames.Count);
                    Value.Name = names.MaleNames[index];
                }
                else if (Value.Gender == GenderType.Female && names.FemaleNames.Count() > 0)
                {
                    int index = _random.Next(0, names.FemaleNames.Count);
                    Value.Name = names.FemaleNames[index];
                }
            }
        }

        private CultureNames GetNameSource()
        {
            // 首先检查是否通过NameSource属性绑定了名字源
            CultureNames names=new CultureNames();
            if (NameSource != null && NameSource.Count > 0)
            {
                if (Value.Culture == "honeywiner")
                {
                    names = NameSource.FirstOrDefault(x => x.CultureName == "reachman");
                }
                else
                {
                    names = NameSource.FirstOrDefault(x => x.CultureName == Value.Culture);
                }                 
            }

            return names;      
        }

        private object FindDataContext()
        {
            // 尝试获取DataContext
            var dataContext = this.DataContext;
            if (dataContext != null)
                return dataContext;

            // 尝试从父元素获取DataContext
            var parent = Parent as FrameworkElement;
            while (parent != null)
            {
                if (parent.DataContext != null)
                    return parent.DataContext;

                parent = parent.Parent as FrameworkElement;
            }

            // 最后尝试从应用程序主窗口获取DataContext
            return Application.Current.MainWindow?.DataContext;
        }

        public FrameworkElement ResolveEditor(PropertyItem propertyItem)
        {
            // 创建值绑定
            Binding binding = new Binding("Value");
            binding.Source = propertyItem;
            binding.Mode = propertyItem.IsReadOnly ? BindingMode.OneWay : BindingMode.TwoWay;

            // 应用绑定
            BindingOperations.SetBinding(this, RandomNameEditor.ValueProperty, binding);

            // 尝试绑定名字源
            TryBindNameSource(propertyItem);

            return this;
        }

        private void TryBindNameSource(PropertyItem propertyItem)
        {
            // 检查是否有RandomNameAttribute特性
            var randomNameAttribute = propertyItem.PropertyDescriptor.Attributes
                .OfType<RandomNameAttribute>()
                .FirstOrDefault();

            if (randomNameAttribute != null && !string.IsNullOrEmpty(randomNameAttribute.NameSourceProperty))
            {
                // 从指定的属性绑定名字源
                Binding nameSourceBinding = new Binding(randomNameAttribute.NameSourceProperty);
                nameSourceBinding.Source = FindDataContext();
                BindingOperations.SetBinding(this, RandomNameEditor.NameSourceProperty, nameSourceBinding);
            }
        }
    }

    // 自定义特性，用于指定使用RandomNameEditor作为编辑器
    [AttributeUsage(AttributeTargets.Property)]
    public class RandomNameAttribute : Attribute
    {
        public List<CultureNames> NameSource { get; set; }

        // 添加NameSourceProperty属性，用于指定绑定的名字源属性名
        public string NameSourceProperty { get; set; }

        public RandomNameAttribute()
        {
            NameSource = new List<CultureNames>();
            NameSourceProperty = string.Empty;
        }
    }
}
