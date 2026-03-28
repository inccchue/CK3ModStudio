using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WpfPrismFrameworkTemplate.Model;
using WpfPrismFrameworkTemplate.ViewModels;
using AdonisUI.Controls;

namespace WpfPrismFrameworkTemplate.Views
{
	/// <summary>
	/// MainWindow.xaml 的交互逻辑
	/// </summary>
	public partial class MainWindow :  AdonisWindow
	{
		public MainWindow()
		{
			InitializeComponent();
		}

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                if (e.NewValue is People)
                    viewModel.SelectPeople = (People)e.NewValue;
                else if (e.NewValue is Family)
                    viewModel.SelectFamily = (Family)e.NewValue;
            }
        }

        private void TreeView_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var vm = DataContext as MainWindowViewModel;
            if (vm == null) return;

            var treeView = (TreeView)sender;
            var item = GetTreeViewItemAtPoint(e.OriginalSource as DependencyObject);

            ContextMenu cm = new ContextMenu();

            if (item != null)
            {
                // 右键点击节点：直接同步更新 ViewModel，不依赖 SelectedItemChanged 事件
                item.IsSelected = true;
                if (item.DataContext is Family family)
                {
                    vm.SelectFamily = family;
                    cm.Items.Add(new MenuItem { Header = "添加成员", Command = vm.AddPeopleCmd });
                    cm.Items.Add(new MenuItem { Header = "删除家族", Command = vm.DelFamilyCmd });
                }
                else if (item.DataContext is People people)
                {
                    vm.SelectPeople = people;
                    cm.Items.Add(new MenuItem { Header = "删除成员", Command = vm.DelPeopleCmd });
                }
            }
            else
            {
                // 右键点击空白区域
                cm.Items.Add(new MenuItem { Header = "添加家族", Command = vm.AddFamilyCmd });
            }

            treeView.ContextMenu = cm;
        }

        private TreeViewItem GetTreeViewItemAtPoint(DependencyObject source)
        {
            while (source != null && !(source is TreeViewItem))
                source = VisualTreeHelper.GetParent(source);
            return source as TreeViewItem;
        }

        private void AutoSuggestBox_TextChanged(ModernWpf.Controls.AutoSuggestBox sender, ModernWpf.Controls.AutoSuggestBoxTextChangedEventArgs args)
        {
           
            if (DataContext is MainWindowViewModel viewModel)
            {
                if (sender.Name.Contains("Mom"))
                {
                    viewModel.HandleTextChanged(new ParentChangedEventArgs { Text = sender.Text, Reason = args.Reason, TargetPeople = viewModel.SelectPeople, SearchType =SearchType.Mom });
                }
                else if(sender.Name.Contains("Dad"))
                {
                    viewModel.HandleTextChanged(new ParentChangedEventArgs { Text = sender.Text, Reason = args.Reason, TargetPeople = viewModel.SelectPeople, SearchType = SearchType.Dad });
                }
                else
                {
                    viewModel.HandleTextChanged(new ParentChangedEventArgs { Text = sender.Text, Reason = args.Reason, TargetPeople = viewModel.SelectPeople, SearchType = SearchType.Spouse });
                }
            }
        }

        private void AutoSuggestBox_QuerySubmitted(ModernWpf.Controls.AutoSuggestBox sender, ModernWpf.Controls.AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                if (sender.Name.Contains("Mom"))
                {
                    viewModel.HandleQuerySubmitted(new QuerySubmittedEventArgs
                    {
                        QueryText = args.QueryText,
                        ChosenSuggestion = args.ChosenSuggestion,
                        TargetPeople = viewModel.SelectPeople,
                        SearchType = SearchType.Mom
                    }); 
                }
                else if(sender.Name.Contains("Dad"))
                {
                    viewModel.HandleQuerySubmitted(new QuerySubmittedEventArgs
                    {
                        QueryText = args.QueryText,
                        ChosenSuggestion = args.ChosenSuggestion,
                        TargetPeople = viewModel.SelectPeople,
                        SearchType = SearchType.Dad
                    });
                }
                else
                {
                    viewModel.HandleQuerySubmitted(new QuerySubmittedEventArgs
                    {
                        QueryText = args.QueryText,
                        ChosenSuggestion = args.ChosenSuggestion,
                        TargetPeople = viewModel.SelectPeople,
                        SearchType = SearchType.Spouse
                    });
                }
                
            }
        }

        private void AutoSuggestBox_SuggestionChosen(ModernWpf.Controls.AutoSuggestBox sender, ModernWpf.Controls.AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                viewModel.HandleSuggestionChosen(args.SelectedItem);
            }
        }
    }
}
