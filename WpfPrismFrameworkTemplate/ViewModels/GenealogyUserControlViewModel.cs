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
    public class GenealogyUserControlViewModel : BindableBase, INavigationAware
    {
        public ObservableCollection<Family> _FamilyList = new ObservableCollection<Family>();
        public ObservableCollection<Family> FamilyList
        {
            get => _FamilyList;
            set
            {
                SetProperty(ref _FamilyList, value);
            }
        }
        public GenealogyUserControlViewModel()
        {

        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (navigationContext.Parameters.ContainsKey("FamilyList"))
            {
                FamilyList = navigationContext.Parameters.GetValue<ObservableCollection<Family>>("FamilyList");
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
