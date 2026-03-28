using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using WpfPrismFrameworkTemplate.Model;

namespace WpfPrismFrameworkTemplate.ViewModels
{
    public class FileReadWriteUserControlViewModel : BindableBase, INavigationAware
    {
        private FileReadWrite _fileReadWrite;

        
        public FileReadWriteUserControlViewModel()
        {
            _fileReadWrite = new FileReadWrite();
        }

        public FileReadWrite FileReadWrite
        {
            get => _fileReadWrite;
            set => SetProperty(ref _fileReadWrite, value);
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (navigationContext.Parameters.ContainsKey("FileReadWrite"))
            {
                FileReadWrite = navigationContext.Parameters.GetValue<FileReadWrite>("FileReadWrite");
            }
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
