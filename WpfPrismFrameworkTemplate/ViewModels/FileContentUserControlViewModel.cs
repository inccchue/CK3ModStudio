using System;
using System.Collections.Generic;
using System.Linq;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using WpfPrismFrameworkTemplate.Model;

namespace WpfPrismFrameworkTemplate.ViewModels
{
    public class FileContentUserControlViewModel : BindableBase, INavigationAware
    {
        public FileContentUserControlViewModel()
        {

        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            //if (navigationContext.Parameters.ContainsKey("FileReadWrite"))
            //{
            //    FileReadWrite = navigationContext.Parameters.GetValue<FileReadWrite>("FileReadWrite");
            //}
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }
    }
}
