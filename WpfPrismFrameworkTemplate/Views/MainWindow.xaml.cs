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
                {
                    viewModel.SelectPeople = (People)e.NewValue;
                }
                else if (e.NewValue is Family)
                {
                    viewModel.SelectFamily = (Family)e.NewValue;
                }
                
            }
        }

        private void AutoSuggestBox_TextChanged(ModernWpf.Controls.AutoSuggestBox sender, ModernWpf.Controls.AutoSuggestBoxTextChangedEventArgs args)
        {
           
            if (DataContext is MainWindowViewModel viewModel)
            {
                if (sender.Name == "MomAutoSuggestBox")
                {
                    viewModel.HandleTextChanged(new ParentChangedEventArgs { Text = sender.Text, Reason = args.Reason, IsMom = true });
                }
                else
                {
                    viewModel.HandleTextChanged(new ParentChangedEventArgs { Text = sender.Text, Reason = args.Reason, IsMom = false });
                }
                
            }
        }

        private void AutoSuggestBox_QuerySubmitted(ModernWpf.Controls.AutoSuggestBox sender, ModernWpf.Controls.AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                if (sender.Name == "MomAutoSuggestBox")
                {
                    viewModel.HandleQuerySubmitted(new QuerySubmittedEventArgs
                    {
                        QueryText = args.QueryText,
                        ChosenSuggestion = args.ChosenSuggestion,
                        TargetPeople = viewModel.SelectPeople,
                        IsMom = true
                    }); 
                }
                else
                {
                    viewModel.HandleQuerySubmitted(new QuerySubmittedEventArgs
                    {
                        QueryText = args.QueryText,
                        ChosenSuggestion = args.ChosenSuggestion,
                        TargetPeople = viewModel.SelectPeople,
                        IsMom = false
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
