using System.Windows;
using WpfPrismFrameworkTemplate.ViewModels;

namespace WpfPrismFrameworkTemplate.Views
{
    public partial class MoveLandedTitleDialog : Window
    {
        public MoveLandedTitleViewModel VM { get; }

        public MoveLandedTitleDialog(MoveLandedTitleViewModel vm)
        {
            VM = vm;
            DataContext = vm;
            InitializeComponent();
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
