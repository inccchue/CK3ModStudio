using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq;
using System.Security.Cryptography;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using WpfPrismFrameworkTemplate.Model;
using WpfPrismFrameworkTemplate.Views;

namespace WpfPrismFrameworkTemplate.ViewModels
{
    public class GenealogyUserControlViewModel : BindableBase, INavigationAware
    {
        public Family _RootFamily = new Family();
        private People _SelectPeople;
        private IDialogService _dialogService;
        private bool _IsDrawerOpen = false;
        private LifeEvent _SelectLifeEvent = new LifeEvent();
        private ObservableCollection<People> _familyTree = new ObservableCollection<People>();
        private ObservableCollection<string> _religionOptions = new ObservableCollection<string>();
        private ObservableCollection<string> _CultureOptions = new ObservableCollection<string>();
        public DelegateCommand<People> AddNewChildCmd { get; private set; }
        public DelegateCommand<People> DelCmd { get; private set; }
        public DelegateCommand ModifyCmd { get; private set; }
        public DelegateCommand AddLifeEventCmd { get; private set; }
        public DelegateCommand DelLifeEventCmd { get; private set; }
        public DelegateCommand ModifyLifeEventCmd { get; private set; }
        public DelegateCommand AddReligionCommand { get; private set; }
        public DelegateCommand AddCultureCommand { get; private set; }

        public GenealogyUserControlViewModel(IDialogService dialogService)
        {
            _dialogService = dialogService;
            AddNewChildCmd = new DelegateCommand<People>(AddNewChild);
            DelCmd = new DelegateCommand<People>(Del);
            ModifyCmd = new DelegateCommand(Modify);
            AddLifeEventCmd = new DelegateCommand(AddLifeEvent);
            DelLifeEventCmd = new DelegateCommand(DelLifeEvent);
            ModifyLifeEventCmd = new DelegateCommand(ModifyLifeEvent);
            AddReligionCommand = new DelegateCommand(AddReligionExecute);
            AddCultureCommand = new DelegateCommand(AddCultureExecute);
        }

        public ObservableCollection<string> CultureOptions
        {
            get => _CultureOptions;
            set => SetProperty(ref _CultureOptions, value);
        }
        public ObservableCollection<string> ReligionOptions
        {
            get => _religionOptions;
            set => SetProperty(ref _religionOptions, value);
        }
        public LifeEvent SelectLifeEvent
        {
            get => _SelectLifeEvent;
            set => SetProperty(ref _SelectLifeEvent, value);
        }
        public bool IsDrawerOpen
        {
            get => _IsDrawerOpen;
            set
            {

                SetProperty(ref _IsDrawerOpen, value);
            }
        }
        public People SelectPeople
        {
            get => _SelectPeople;
            set
            {

                SetProperty(ref _SelectPeople, value);
            }
        }

        public Family RootFamily
        {
            get => _RootFamily;
            set
            {
                SetProperty(ref _RootFamily, value);
            }
        }

        public ObservableCollection<People> FamilyTree
        {
            get => _familyTree;
            set => SetProperty(ref _familyTree, value);
        }

        private void AddReligionExecute()
        {
            dynamic obj = new ExpandoObject();
            obj.宗教名 = "";
            DialogParameters parm = new DialogParameters
    {
        { "value", obj }
    };
            _dialogService.ShowDialog(nameof(AssignmentWindow), parm, arg =>
            {
                if (arg.Result == ButtonResult.OK)
                {
                    obj = arg.Parameters.GetValue<ExpandoObject>("value");
                }
            });
            if (!ReligionOptions.Contains(obj.宗教名) && !string.IsNullOrWhiteSpace(obj.宗教名))
            {
                ReligionOptions.Add(obj.宗教名);  // 如果不存在，则添加
            }
        }
        private void AddCultureExecute()
        {
            dynamic obj = new ExpandoObject();
            obj.文化名 = "";
            DialogParameters parm = new DialogParameters
    {
        { "value", obj }
    };
            _dialogService.ShowDialog(nameof(AssignmentWindow), parm, arg =>
            {
                if (arg.Result == ButtonResult.OK)
                {
                    obj = arg.Parameters.GetValue<ExpandoObject>("value");
                }
            });
            // 判断CultureOptions中是否已经存在相同的文化名
            if (!CultureOptions.Contains(obj.文化名) && !string.IsNullOrWhiteSpace(obj.文化名))
            {
                CultureOptions.Add(obj.文化名);  // 如果不存在，则添加
            }

        }

        private void ModifyLifeEvent()
        {
            if (SelectPeople == null || SelectLifeEvent == null)
            {
                return;
            }

            DialogParameters parm = new DialogParameters
    {
                { "value", SelectLifeEvent }
    };
            _dialogService.ShowDialog(nameof(CommonAssignmentWindow), parm, arg =>
            {

            });
        }
        private void DelLifeEvent()
        {
            if (SelectPeople == null || SelectLifeEvent == null)
            {
                return;
            }

            SelectPeople.LifeEventList.Remove(SelectLifeEvent);
        }
        private void AddLifeEvent()
        {
            if (SelectPeople == null)
            {
                return;
            }
            dynamic obj = new ExpandoObject();
            obj.事件类型 = LifeEventType.Death;
            DialogParameters parm = new DialogParameters
        {
            { "value", obj }
        };
            _dialogService.ShowDialog(nameof(AssignmentWindow), parm, arg =>
            {
                if (arg.Result == ButtonResult.OK)
                {
                    obj = arg.Parameters.GetValue<ExpandoObject>("value");
                }
            });

            LifeEvent tmpLifeEvent = LifeEventFactory.CreateLifeEvent(obj.事件类型);

            parm = new DialogParameters
    {
        { "value", tmpLifeEvent }
    };
            _dialogService.ShowDialog(nameof(CommonAssignmentWindow), parm, arg =>
            {
                if (arg.Result == ButtonResult.OK)
                {
                    tmpLifeEvent = arg.Parameters.GetValue<LifeEvent>("value");
                }
            });
            SelectPeople.LifeEventList.Add(tmpLifeEvent);
        }
        public void Modify()
        {
            if (SelectPeople != null)
            {
                IsDrawerOpen=!IsDrawerOpen;
            }
        }
        public void Del(People people)
        {
            if (people != null)
            {
                if (people.Dad != null)
                {
                    people.Dad.Children.Remove(people);
                }
                else
                {
                    RootFamily.Members.Remove(people);
                }
            }
        }
        public void AddNewChild(People parent)
        {
            if (parent != null)
            {
                dynamic obj = new ExpandoObject();
                obj.性别 = GenderType.Male;
                obj.名字 = "";
                DialogParameters parm = new DialogParameters
        {
            { "value", obj }
        };
                _dialogService.ShowDialog(nameof(AssignmentWindow), parm, arg =>
                {
                    if (arg.Result == ButtonResult.OK)
                    {
                        obj = arg.Parameters.GetValue<ExpandoObject>("value");
                    }
                });
                People newPeople = new People(RootFamily.FindMemberWithMaxIdNumber() + 1, obj.名字, obj.性别, RootFamily.FamilyName,parent.Religion,parent.Culture);
                newPeople.Dad = parent;
                parent.Children.Add(newPeople);
            }
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (navigationContext.Parameters.ContainsKey("RootFamily"))
            {
                RootFamily = navigationContext.Parameters.GetValue<Family>("RootFamily");
                CultureOptions = navigationContext.Parameters.GetValue<ObservableCollection<string>>("CultureOptions");
                ReligionOptions = navigationContext.Parameters.GetValue<ObservableCollection<string>>("ReligionOptions");
                UpdateMembersWithoutDad();
                RootFamily.Members.CollectionChanged += (s, e) =>UpdateMembersWithoutDad();
            }
        }

        private void UpdateMembersWithoutDad()
        {
            FamilyTree.Clear();
            foreach (var person in RootFamily.Members.Where(p => p.Dad == null))
            {
                FamilyTree.Add(person);
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
