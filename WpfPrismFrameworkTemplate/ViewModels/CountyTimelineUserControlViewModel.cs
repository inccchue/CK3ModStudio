using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq;
using System.Windows.Media;
using ModernWpf.Controls;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using Windows.Gaming.Preview.GamesEnumeration;
using WpfPrismFrameworkTemplate.Helper;
using WpfPrismFrameworkTemplate.Model;
using WpfPrismFrameworkTemplate.Views;

namespace WpfPrismFrameworkTemplate.ViewModels
{
    public enum InsertType
    {
        在上方插入,
        在下方插入
    }
    public class CountyTimelineUserControlViewModel : BindableBase, INavigationAware
    {
        ObservableCollection<Family> _FamilyList=new ObservableCollection<Family>();
        private Family _SelectFamily = new Family();
        private IDialogService _dialogService;
        private IEventAggregator _eventAggregator;
        private FileReadWrite _fileReadWrite;
        private County _SelectCounty = new County();
        private ObservableCollection<County> _counties=new ObservableCollection<County>();
        public ObservableCollection<TimelineEntry> _TimelineEntries = new ObservableCollection<TimelineEntry>();
        private ObservableCollection<County> _suggestions = new ObservableCollection<County>();
        FamilyInheritanceManager familyInheritanceManager = new FamilyInheritanceManager();
        public DelegateCommand<TimelineEntry> DelCmd { get; private set; }
        public DelegateCommand<TimelineEntry> AddCmd { get; private set; }
        public DelegateCommand SelectChangeCmd { get; private set; }
        public DelegateCommand SaveCmd { get; private set; }
        public DelegateCommand FitCmd { get; private set; }
        public CountyTimelineUserControlViewModel(IDialogService dialogService, IEventAggregator eventAggregator)
        {
            DelCmd = new DelegateCommand<TimelineEntry>(Del);
            AddCmd = new DelegateCommand<TimelineEntry>(Add);
            SaveCmd = new DelegateCommand(Save);
            FitCmd = new DelegateCommand(Fit);
            SelectChangeCmd = new DelegateCommand(SelectChange);
            _dialogService = dialogService;
            _eventAggregator = eventAggregator;
        }

        public Family SelectFamily
        {
            get => _SelectFamily;
            set
            {
                SetProperty(ref _SelectFamily, value);

                
            }
        }
        public ObservableCollection<Family> FamilyList
        {
            get => _FamilyList;
            set => SetProperty(ref _FamilyList, value);
        }
        public ObservableCollection<County> Suggestions
        {
            get => _suggestions;
            set => SetProperty(ref _suggestions, value);
        }
        public FileReadWrite FileReadWrite
        {
            get => _fileReadWrite;
            set => SetProperty(ref _fileReadWrite, value);
        }
        public County SelectCounty
        {
            get => _SelectCounty;
            set 
            { 
                SetProperty(ref _SelectCounty, value);
                ProcessTimelineEntries();
            }
        }
        public ObservableCollection<County> Counties
        {
            get => _counties;
            set => SetProperty(ref _counties, value);
        }
        public ObservableCollection<TimelineEntry> TimelineEntries
        {
            get => _TimelineEntries;
            set => SetProperty(ref _TimelineEntries, value);
        }

        public void Fit()
        {
            try
            {
                SelectCounty.HolderEntries = familyInheritanceManager.UpdateHolderEntries(SelectCounty.HolderEntries, SelectFamily.Members);
                ProcessTimelineEntries();
                HandyControl.Controls.Growl.Success("家族贴合时间轴成功");
            }
            catch (Exception ex)
            {
                HandyControl.Controls.Growl.Error($@"家族贴合时间轴失败,失败原因是{ex.Message}");
            }
        }
        public void SelectChange()
        {
            _eventAggregator.GetEvent<SelectFamilyChangeEvent>().Publish(SelectFamily);
        }
        public void Save()
        {
            if (FileReadWrite != null)
            {
                CountyParser.SaveCountiesToFile(Counties, FileReadWrite.DomainDefFile);
                HandyControl.Controls.Growl.Success("保存文件成功");
            }
        }
        public void Add(TimelineEntry timelineEntry)
        {
            dynamic obj = new ExpandoObject();
            obj.新时间节点插入位置 = InsertType.在上方插入;
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

            HolderEntry newHolderEntry = new HolderEntry();
            if (obj.新时间节点插入位置 == InsertType.在上方插入)
            {
                newHolderEntry.StartDate = timelineEntry.StartDate;
            }
            else
            {
                newHolderEntry.StartDate = timelineEntry.EndDate;
            }
            newHolderEntry.Holder = timelineEntry.Holder;

            parm = new DialogParameters
    {
        { "value", newHolderEntry }
    };
            _dialogService.ShowDialog(nameof(CommonAssignmentWindow), parm, arg =>
            {
                if (arg.Result == ButtonResult.OK)
                {
                    newHolderEntry = arg.Parameters.GetValue<HolderEntry>("value");
                }
            });
            SelectCounty.HolderEntries.Add(newHolderEntry);
            ProcessTimelineEntries();
        }
        public void Del(TimelineEntry timelineEntry)
        {
            TimelineEntries.Remove(timelineEntry);
            SelectCounty.HolderEntries.Remove(SelectCounty.HolderEntries.FirstOrDefault(e => e.StartDate == timelineEntry.StartDate && e.Holder == timelineEntry.Holder));
            ProcessTimelineEntries();
        }
        public void HandleTextChanged(CountyChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                List<County> filtered;

                // 过滤逻辑
                filtered = string.IsNullOrEmpty(args.Text)
                ? new List<County>()
                : Counties.Where(p =>
                    (!string.IsNullOrEmpty(p.Name) && p.Name.IndexOf(args.Text, StringComparison.OrdinalIgnoreCase) >= 0)
                ).ToList();


                Suggestions.Clear();
                foreach (var person in filtered)
                {
                    Suggestions.Add(person);
                }
            }
        }

        public void HandleQuerySubmitted(CountyQuerySubmittedEventArgs querySubmittedEvent)
        {
            if (querySubmittedEvent.ChosenSuggestion != null && querySubmittedEvent.ChosenSuggestion is County county)
            {
                SelectCounty = county;
            }
            else
            {
                County searchResult;
                searchResult = Counties.FirstOrDefault(p => p.Name == querySubmittedEvent.QueryText);
                SelectCounty = searchResult;

            }
        }

        private void ProcessTimelineEntries()
        {
            TimelineEntries.Clear();

            if (SelectCounty == null || SelectCounty.HolderEntries.Count == 0)
                return;

            var sortedEntries = SelectCounty.HolderEntries.OrderBy(e => e.StartDate).ToList();

            // 创建随机颜色生成器
            Random random = new Random(42); // 固定种子以保持颜色一致性

            for (int i = 0; i < sortedEntries.Count; i++)
            {
                var entry = sortedEntries[i];
                string endDate = (i < sortedEntries.Count - 1) ? sortedEntries[i + 1].StartDate : "至今";

                // 创建随机颜色
                Color color = Color.FromRgb(
                    (byte)random.Next(100, 240),
                    (byte)random.Next(100, 240),
                    (byte)random.Next(100, 240));

                TimelineEntries.Add(new TimelineEntry
                {
                    StartDate = entry.StartDate,
                    EndDate = endDate,
                    Holder = entry.Holder,
                    Background = new SolidColorBrush(color)
                });
            }
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (navigationContext.Parameters.ContainsKey("FileReadWrite")
                && navigationContext.Parameters.ContainsKey("FamilyList")
                && navigationContext.Parameters.ContainsKey("SelectFamily"))
            {
                FileReadWrite = navigationContext.Parameters.GetValue<FileReadWrite>("FileReadWrite");
                FamilyList = navigationContext.Parameters.GetValue<ObservableCollection<Family>>("FamilyList");
                SelectFamily = navigationContext.Parameters.GetValue<Family>("SelectFamily");
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
