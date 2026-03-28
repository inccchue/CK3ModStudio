using System.Windows.Controls;
using WpfPrismFrameworkTemplate.Model;
using WpfPrismFrameworkTemplate.ViewModels;

namespace WpfPrismFrameworkTemplate.Views
{
    /// <summary>
    /// Interaction logic for GenealogyUserControl
    /// </summary>
    public partial class GenealogyUserControl : UserControl
    {
        public GenealogyUserControl()
        {
            InitializeComponent();
        }

        private void TreeView_SelectedItemChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<object> e)
        {
            if (DataContext is GenealogyUserControlViewModel viewModel)
            {
                if (e.NewValue is People)
                {
                    viewModel.SelectPeople = (People)e.NewValue;
                }

            }
        }

        private void AutoSuggestBox_TextChanged(ModernWpf.Controls.AutoSuggestBox sender, ModernWpf.Controls.AutoSuggestBoxTextChangedEventArgs args)
        {

            if (DataContext is GenealogyUserControlViewModel viewModel)
            {
                if (sender.Name.Contains("Mom"))
                {
                    viewModel.SendHandleTextChanged(sender.Text, args.Reason, SearchType.Mom);
                }
                else if (sender.Name.Contains("Dad"))
                {
                    viewModel.SendHandleTextChanged(sender.Text, args.Reason, SearchType.Dad);
                }
                else
                {
                    viewModel.SendHandleTextChanged(sender.Text, args.Reason, SearchType.Spouse);
                }

            }
        }

        private void AutoSuggestBox_QuerySubmitted(ModernWpf.Controls.AutoSuggestBox sender, ModernWpf.Controls.AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (DataContext is GenealogyUserControlViewModel viewModel)
            {
                if (sender.Name.Contains("Mom"))
                {
                    viewModel.SendQuerySubmitted(args.QueryText, args.ChosenSuggestion, SearchType.Mom);
                }
                else if (sender.Name.Contains("Dad"))
                {
                    viewModel.SendQuerySubmitted(args.QueryText, args.ChosenSuggestion, SearchType.Dad);
                }
                else
                {
                    viewModel.SendQuerySubmitted(args.QueryText, args.ChosenSuggestion, SearchType.Spouse);
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
