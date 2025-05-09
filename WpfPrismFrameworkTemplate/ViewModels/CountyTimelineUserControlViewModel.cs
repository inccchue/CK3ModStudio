using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using WpfPrismFrameworkTemplate.Helper;
using WpfPrismFrameworkTemplate.Model;

namespace WpfPrismFrameworkTemplate.ViewModels
{
    public class CountyTimelineUserControlViewModel : BindableBase, INavigationAware
    {
        private FileReadWrite _fileReadWrite;
        private County _SelectCounty = new County();
        private ObservableCollection<County> _counties=new ObservableCollection<County>();
        public CountyTimelineUserControlViewModel()
        {

        }

        public FileReadWrite FileReadWrite
        {
            get => _fileReadWrite;
            set => SetProperty(ref _fileReadWrite, value);
        }
        public County SelectCounty
        {
            get => _SelectCounty;
            set => SetProperty(ref _SelectCounty, value);
        }
        public ObservableCollection<County> Counties
        {
            get => _counties;
            set => SetProperty(ref _counties, value);
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (navigationContext.Parameters.ContainsKey("FileReadWrite"))
            {
                FileReadWrite = navigationContext.Parameters.GetValue<FileReadWrite>("FileReadWrite");
                CountyParser.ParseCountiesFromFile(Counties, FileReadWrite.DomainDefFile);
                SelectCounty=Counties.FirstOrDefault();
            }
            else
            {
                FileReadWrite = new FileReadWrite();
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
