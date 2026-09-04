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
                viewModel.HandleTextChanged(new CountyChangedEventArgs { Text = sender.Text, Reason = args.Reason, TargetCounty = viewModel.SelectCounty});
            }
        }

        private void AutoSuggestBox_QuerySubmitted(ModernWpf.Controls.AutoSuggestBox sender, ModernWpf.Controls.AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (DataContext is CountyTimelineUserControlViewModel viewModel)
            {
                viewModel.HandleQuerySubmitted(new CountyQuerySubmittedEventArgs
                {
                    QueryText = args.QueryText,
                    ChosenSuggestion = args.ChosenSuggestion,
                    TargetCounty = viewModel.SelectCounty,
                });
            }
        }

        private void AutoSuggestBox_SuggestionChosen(ModernWpf.Controls.AutoSuggestBox sender, ModernWpf.Controls.AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            
        }
    }
}
