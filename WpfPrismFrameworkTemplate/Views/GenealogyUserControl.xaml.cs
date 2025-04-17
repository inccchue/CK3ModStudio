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
                //if (e.NewValue is People)
                //{
                //    viewModel.SelectPeople = (People)e.NewValue;
                //}
                //else if (e.NewValue is Family)
                //{
                //    viewModel.SelectFamily = (Family)e.NewValue;
                //}

            }
        }
    }
}
