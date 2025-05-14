using System.Windows.Controls;
using WpfPrismFrameworkTemplate.Model;
using WpfPrismFrameworkTemplate.ViewModels;

namespace WpfPrismFrameworkTemplate.Views
{
    /// <summary>
    /// Interaction logic for CountyTimelineUserControl
    /// </summary>
    public partial class CountyTimelineUserControl : UserControl
    {
        public CountyTimelineUserControl()
        {
            InitializeComponent();
        }

        private void AutoSuggestBox_TextChanged(ModernWpf.Controls.AutoSuggestBox sender, ModernWpf.Controls.AutoSuggestBoxTextChangedEventArgs args)
        {

            if (DataContext is CountyTimelineUserControlViewModel viewModel)
            {
                viewModel.HandleTextChanged(new ParentChangedEventArgs { Text = sender.Text, Reason = args.Reason, TargetPeople = viewModel.SelectPeople, SearchType = SearchType.Mom });
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
                else if (sender.Name.Contains("Dad"))
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
