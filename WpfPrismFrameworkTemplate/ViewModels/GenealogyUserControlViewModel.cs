using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq;
using System.Security.Cryptography;
using ModernWpf.Controls;
using Prism.Commands;
using Prism.Events;
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
        private IEventAggregator _eventAggregator;
        private bool _IsDrawerOpen = false;
        private LifeEvent _SelectLifeEvent = new LifeEvent();
        private ObservableCollection<People> _familyTree = new ObservableCollection<People>();
        private ObservableCollection<string> _religionOptions = new ObservableCollection<string>();
        private ObservableCollection<string> _CultureOptions = new ObservableCollection<string>();
        private ObservableCollection<People> _suggestions = new ObservableCollection<People>();
        public DelegateCommand<People> AddNewChildCmd { get; private set; }
        public DelegateCommand<People> DelCmd { get; private set; }
        public DelegateCommand ModifyCmd { get; private set; }
        public DelegateCommand AddLifeEventCmd { get; private set; }
        public DelegateCommand DelLifeEventCmd { get; private set; }
        public DelegateCommand ModifyLifeEventCmd { get; private set; }
        public DelegateCommand AddReligionCommand { get; private set; }
        public DelegateCommand AddCultureCommand { get; private set; }

        public GenealogyUserControlViewModel(IDialogService dialogService,IEventAggregator eventAggregator)
        {
            _dialogService = dialogService;
            _eventAggregator = eventAggregator;
            AddNewChildCmd = new DelegateCommand<People>(AddNewChild);
            DelCmd = new DelegateCommand<People>(Del);
            ModifyCmd = new DelegateCommand(Modify);
            AddLifeEventCmd = new DelegateCommand(AddLifeEvent);
            DelLifeEventCmd = new DelegateCommand(DelLifeEvent);
            ModifyLifeEventCmd = new DelegateCommand(ModifyLifeEvent);
            AddReligionCommand = new DelegateCommand(AddReligionExecute);
            AddCultureCommand = new DelegateCommand(AddCultureExecute);
        }

        public ObservableCollection<People> Suggestions
        {
            get => _suggestions;
            set => SetProperty(ref _suggestions, value);
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

        public void SendHandleTextChanged(string text, AutoSuggestionBoxTextChangeReason reason, SearchType searchType)
        {
            var args = new ParentChangedEventArgs
            {
                Text = text,
                Reason = reason,
                TargetPeople = SelectPeople,
                SearchType = searchType
            };
            _eventAggregator.GetEvent<ParentChangedEvent>().Publish(args);
        }

        public void SendQuerySubmitted(string queryText, object chosenSuggestion, SearchType searchType)
        {
            var args = new QuerySubmittedEventArgs
            {
                QueryText = queryText,
                ChosenSuggestion = chosenSuggestion,
                TargetPeople = SelectPeople,
                SearchType = searchType
            };
            _eventAggregator.GetEvent<QuerySubmittedEvent>().Publish(args);
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
            LifeEvent tmpLifeEvent = new LifeEvent();

            DialogParameters parm = new DialogParameters
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
                if (people.Mom != null)
                {
                    people.Mom.Children.Remove(people);
                }
                if (people.Dad != null)
                {
                    people.Dad.Children.Remove(people);
                }
                if (people.Spouse != null)
                {
                    people.Spouse.LifeEventList.Remove(people.Spouse.LifeEventList.FirstOrDefault(e => e.EventType == LifeEventType.Marriage));
                    people.Spouse.Spouse = null;
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
            if (navigationContext.Parameters.ContainsKey("RootFamily")
                && navigationContext.Parameters.ContainsKey("CultureOptions")
                && navigationContext.Parameters.ContainsKey("ReligionOptions")
                && navigationContext.Parameters.ContainsKey("Suggestions"))
            {
                RootFamily = navigationContext.Parameters.GetValue<Family>("RootFamily");
                CultureOptions = navigationContext.Parameters.GetValue<ObservableCollection<string>>("CultureOptions");
                ReligionOptions = navigationContext.Parameters.GetValue<ObservableCollection<string>>("ReligionOptions");
                Suggestions = navigationContext.Parameters.GetValue<ObservableCollection<People>>("Suggestions");
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
