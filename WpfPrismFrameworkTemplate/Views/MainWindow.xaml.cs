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

    }
}
