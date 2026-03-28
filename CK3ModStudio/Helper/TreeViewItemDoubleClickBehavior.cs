using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Xaml.Behaviors;

namespace WpfPrismFrameworkTemplate.Helper
{
    public class TreeViewDoubleClickBehavior : Behavior<TreeView>
    {
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(TreeViewDoubleClickBehavior));

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            // 使用隧道事件（Preview事件）来确保我们能在默认处理之前拦截它
            AssociatedObject.AddHandler(TreeViewItem.PreviewMouseDoubleClickEvent,
                new MouseButtonEventHandler(TreeViewItem_PreviewMouseDoubleClick), true);

            // 同时也处理常规的双击事件作为备份
            AssociatedObject.AddHandler(TreeViewItem.MouseDoubleClickEvent,
                new MouseButtonEventHandler(TreeViewItem_MouseDoubleClick), true);

            // 为TreeView中的所有TreeViewItem项添加样式处理
            AssociatedObject.Loaded += AssociatedObject_Loaded;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.RemoveHandler(TreeViewItem.PreviewMouseDoubleClickEvent,
                new MouseButtonEventHandler(TreeViewItem_PreviewMouseDoubleClick));
            AssociatedObject.RemoveHandler(TreeViewItem.MouseDoubleClickEvent,
                new MouseButtonEventHandler(TreeViewItem_MouseDoubleClick));
            AssociatedObject.Loaded -= AssociatedObject_Loaded;
            base.OnDetaching();
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            // 获取所有的TreeViewItem并添加事件处理
            ApplyToAllTreeViewItems(AssociatedObject);
        }

        private void ApplyToAllTreeViewItems(DependencyObject parent)
        {
            // 递归查找所有TreeViewItem
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is TreeViewItem item)
                {
                    // 为每个TreeViewItem添加双击处理
                    item.PreviewMouseDoubleClick += TreeViewItem_PreviewMouseDoubleClick;
                    item.MouseDoubleClick += TreeViewItem_MouseDoubleClick;
                }

                ApplyToAllTreeViewItems(child);
            }
        }

        private void TreeViewItem_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // 在预览阶段就标记为已处理，可以更有效地阻止默认行为
            e.Handled = true;

            if (Command != null && Command.CanExecute(null))
            {
                Command.Execute(null);
            }
        }

        private void TreeViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // 确保事件被标记为已处理
            e.Handled = true;
        }
    }
}
