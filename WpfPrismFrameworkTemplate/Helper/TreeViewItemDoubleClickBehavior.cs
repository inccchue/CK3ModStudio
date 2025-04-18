using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace WpfPrismFrameworkTemplate.Helper
{
    public class TreeViewItemDoubleClickBehavior : Behavior<TreeViewItem>
    {
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(TreeViewItemDoubleClickBehavior), new PropertyMetadata(null));

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.MouseDoubleClick += AssociatedObject_MouseDoubleClick;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.MouseDoubleClick -= AssociatedObject_MouseDoubleClick;
            base.OnDetaching();
        }

        private void AssociatedObject_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && Command != null)
            {
                TreeViewItem treeViewItem = sender as TreeViewItem;
                if (treeViewItem != null)
                {
                    var people = treeViewItem.DataContext;
                    if (Command.CanExecute(people))
                    {
                        Command.Execute(people);
                        e.Handled = true;
                    }
                }
            }
        }
    }
}
