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
                if (sender.Name == "MomAutoSuggestBox")
                {
                    viewModel.SendHandleTextChanged(sender.Text, args.Reason,true);
                }
                else
                {
                    viewModel.SendHandleTextChanged(sender.Text, args.Reason, false);
                }

            }
        }

        private void AutoSuggestBox_QuerySubmitted(ModernWpf.Controls.AutoSuggestBox sender, ModernWpf.Controls.AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (DataContext is GenealogyUserControlViewModel viewModel)
            {
                if (sender.Name == "MomAutoSuggestBox")
                {
                    viewModel.SendQuerySubmitted(args.QueryText, args.ChosenSuggestion);
                }
                else
                {
                    viewModel.SendQuerySubmitted(args.QueryText, args.ChosenSuggestion, false);
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
