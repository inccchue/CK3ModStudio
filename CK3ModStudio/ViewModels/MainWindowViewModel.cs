using Microsoft.Win32;
using ModernWpf.Controls;
using PipeCommunicationLibrary;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Navigation;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using WpfPrismFrameworkTemplate.Helper;
using WpfPrismFrameworkTemplate.Model;
using WpfPrismFrameworkTemplate.Views;

namespace WpfPrismFrameworkTemplate.ViewModels
{
    public class MainWindowViewModel : BindableBase, IDestructible
    {
        public ObservableCollection<People> _FemaleList = new ObservableCollection<People>() { new People()};
        public ObservableCollection<People> _MaleList = new ObservableCollection<People>() { new People()};
        public ObservableCollection<Family> _FamilyList =new ObservableCollection<Family>();
        private ObservableCollection<string> _religionOptions = new ObservableCollection<string>();
        private ObservableCollection<string> _CultureOptions = new ObservableCollection<string>();
        private ObservableCollection<People> _suggestions=new ObservableCollection<People>();
        private ObservableCollection<County> _counties = new ObservableCollection<County>();
        private People _SelectPeople;
        private Family _SelectFamily;
        private string _title = "CK3 Mod Studio (작者:Fred)";
        private readonly FamilyRepository _repository;
        private IEventAggregator _eventAggregator;
        private bool _EnableDatabaseSync = false;
        private IDialogService _dialogService;
        private LifeEvent _SelectLifeEvent=new LifeEvent();
        private readonly IRegionManager _regionManager;
        private string _searchText;
        private FileReadWrite _fileReadWrite;
        private PipeClientService _pipeClientService=new PipeClientService();
        private List<CultureNames> _RandomName = new List<CultureNames>();
        public event Action SelectFamilyChangeOccurred;
        public string _CoAContent = "";
        private BitmapImage _receivedImage;
        private bool _IsPipeServiceConnect = false;

        public DelegateCommand OpenFileCmd { get; private set; }
        public DelegateCommand CreatePersonContentCmd { get; private set; }
        public DelegateCommand CreateFamilyContentCmd { get; private set; }
        public DelegateCommand CreateAllFamilyContentCmd { get; private set; }
        public DelegateCommand AddFamilyCmd { get; private set; }
        public DelegateCommand DelFamilyCmd { get; private set; }
        public DelegateCommand AddPeopleCmd { get; private set; }
        public DelegateCommand DelPeopleCmd { get; private set; }
        public DelegateCommand CloseCommand { get; private set; }
        public DelegateCommand AddReligionCommand { get; private set; }
        public DelegateCommand AddCultureCommand { get; private set; }
        public DelegateCommand OutputPersonFileCommand { get; private set; }
        public DelegateCommand OutputFamilyFileCommand { get; private set; }
        public DelegateCommand OutputAllFamilyFileCommand { get; private set; }
        public DelegateCommand AddLifeEventCmd { get; private set; }
        public DelegateCommand DelLifeEventCmd { get; private set; }
        public DelegateCommand ModifyLifeEventCmd { get; private set; }
        public DelegateCommand FullScreenCmd { get; private set; }
        public DelegateCommand GotoFileSettingCmd { get; private set; }
        public DelegateCommand GotoFileContentCmd { get; private set; }
        public DelegateCommand GotoTimelineCmd { get; private set; }
        public DelegateCommand GotoStatisticsCmd { get; private set; }
        public DelegateCommand GotoDatabaseCmd { get; private set; }
        public DelegateCommand GotoCoaCmd { get; private set; }
        public DelegateCommand GotoLandedTitlesCmd { get; private set; }
        public DelegateCommand LoadedCommand { get; private set; }
        public DelegateCommand SaveAllFileCommand { get; private set; }
        public MainWindowViewModel(IEventAggregator eventAggregator, IDialogService dialogService, IRegionManager regionManager)
		{
            _eventAggregator = eventAggregator;
            _dialogService = dialogService;
            _regionManager = regionManager;
            


            _repository = new FamilyRepository(eventAggregator);

            OpenFileCmd = new DelegateCommand(OpenFile);
            CreatePersonContentCmd = new DelegateCommand(CreatePersonContent);
            CreateFamilyContentCmd = new DelegateCommand(CreateFamilyContent);
            CreateAllFamilyContentCmd = new DelegateCommand(CreateAllFamilyContent);
            AddFamilyCmd = new DelegateCommand(AddFamily);
            AddPeopleCmd = new DelegateCommand(AddPeople);
            DelFamilyCmd = new DelegateCommand(DelFamily);
            DelPeopleCmd = new DelegateCommand(DelPeople);
            CloseCommand = new DelegateCommand(CloseCommandExecute);
            AddReligionCommand = new DelegateCommand(AddReligionExecute);
            AddCultureCommand = new DelegateCommand(AddCultureExecute);
            OutputPersonFileCommand = new DelegateCommand(OutputPersonFile);
            OutputFamilyFileCommand = new DelegateCommand(OutputFamilyFile);
            OutputAllFamilyFileCommand = new DelegateCommand(OutputAllFamilyFile);
            AddLifeEventCmd = new DelegateCommand(AddLifeEvent);
            DelLifeEventCmd = new DelegateCommand(DelLifeEvent);
            ModifyLifeEventCmd = new DelegateCommand(ModifyLifeEvent);
            FullScreenCmd = new DelegateCommand(FullScreen);
            GotoFileSettingCmd = new DelegateCommand(GotoFileSetting);
            GotoFileContentCmd = new DelegateCommand(GotoFileContent);
            LoadedCommand = new DelegateCommand(Loaded);
            SaveAllFileCommand = new DelegateCommand(SaveAllFile);
            GotoTimelineCmd = new DelegateCommand(GotoTimeline);
            GotoCoaCmd = new DelegateCommand(GotoCoa);
            GotoStatisticsCmd = new DelegateCommand(GotoStatistics);
            GotoDatabaseCmd = new DelegateCommand(GotoDatabase);
            GotoLandedTitlesCmd = new DelegateCommand(GotoLandedTitles);
            eventAggregator.GetEvent<SaveMessageEvent>().Subscribe(Save);
            eventAggregator.GetEvent<ParentChangedEvent>().Subscribe(HandleTextChanged);
            eventAggregator.GetEvent<QuerySubmittedEvent>().Subscribe(HandleQuerySubmitted);
            eventAggregator.GetEvent<FileSettingChangeEvent>().Subscribe(LoadPathChangeFileContent);
            eventAggregator.GetEvent<SelectFamilyChangeEvent>().Subscribe(OnFamilyUpdated);
            DebugHelper.Initialize(eventAggregator);
            Load();

           
            SelectFamilyChangeOccurred += () =>
            {
                if (SelectFamily != null)
                {
                    Task.Run(async () =>
                    {
                        try
                        {
                            await PipeClientService.SendFamilyInfoAsync(SelectFamily);
                        }
                        catch (Exception ex)
                        {
                            // 处理异常
                            Console.WriteLine($"发送失败: {ex.Message}");
                        }
                    });
                }
            };
        }

        public bool IsPipeServiceConnect
        {
            get => _IsPipeServiceConnect;
            set => SetProperty(ref _IsPipeServiceConnect, value);
        }
        public BitmapImage ReceivedImage
        {
            get => _receivedImage;
            set => SetProperty(ref _receivedImage, value);
        }
        public string CoAContent
        {
            get => _CoAContent;
            set => SetProperty(ref _CoAContent, value);
        }
        public PipeClientService PipeClientService
        {
            get => _pipeClientService;
            set => SetProperty(ref _pipeClientService, value);
        }
        public ObservableCollection<County> Counties
        {
            get => _counties;
            set => SetProperty(ref _counties, value);
        }
        public FileReadWrite FileReadWrite
        {
            get => _fileReadWrite;
            set => SetProperty(ref _fileReadWrite, value);
        }
        public List<CultureNames> RandomNameList
        {
            get => _RandomName;
            set => SetProperty(ref _RandomName, value);
        }
        public ObservableCollection<People> Suggestions
        {
            get => _suggestions;
            set => SetProperty(ref _suggestions, value);
        }

        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }
        public ObservableCollection<People> MaleList
        {
            get => _MaleList;
            set => SetProperty(ref _MaleList, value);
        }
        public ObservableCollection<People> FemaleList
        {
            get => _FemaleList;
            set => SetProperty(ref _FemaleList, value);
        }
        public LifeEvent SelectLifeEvent
        {
            get => _SelectLifeEvent;
            set => SetProperty(ref _SelectLifeEvent, value);
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
        public bool EnableDatabaseSync
        {
            get => _EnableDatabaseSync;
            set 
            {
                SetProperty(ref _EnableDatabaseSync, value);
                Load();
            }
        }
        public ObservableCollection<Family> FamilyList
        {
            get => _FamilyList;
            set
            {
                SetProperty(ref _FamilyList, value);
            }
        }
        public Family SelectFamily
        {
            get => _SelectFamily;
            set
            {
                SetProperty(ref _SelectFamily, value);
                SelectFamilyChangeOccurred?.Invoke();
            }
        }
        public People SelectPeople
        {
            get => _SelectPeople;
            set
            {
                
                SetProperty(ref _SelectPeople, value);
                GetFamilyForSelectedPeople();
                
            }
        }
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }
        private void OnFamilyUpdated(Family updatedFamily)
        {
            SelectFamily = updatedFamily;
        }

        public void LoadPathChangeFileContent(string changeFile)
        {
            switch (changeFile)
            {
                case nameof(FileReadWrite.RandomNameFilePath):
                    RandomNameList=NameFileParser.ParseCultures(FileReadWrite.RandomNameFilePath);
                    PopulateCultureOptions();
                    break;
                case nameof(FileReadWrite.DomainDefFile):
                    CountyParser.ParseCountiesFromFile(Counties, FileReadWrite.DomainDefFile);
                    break;
                case nameof(FileReadWrite.IsShowDebugInfo):
                    PipeClientService.IsShowDebugInfo = FileReadWrite.IsShowDebugInfo;
                    break;
            }

        }

        public void SaveAllFile()
        {
            _eventAggregator.GetEvent<SaveMessageEvent>().Publish();
        }
        public void Loaded()
        {
            var parameters = new NavigationParameters();
            parameters.Add("FileReadWrite", FileReadWrite);
            parameters.Add("FamilyList", FamilyList);
            _regionManager.RequestNavigate("FileContentRegion", "FileContent", parameters);
        }

        public void HandleTextChanged(ParentChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                // 저장搜索文本
                SearchText = args.Text;
                List<People> filtered;
                UpdateFemaleAndMaleList();

                if (args.SearchType == SearchType.Mom||(args.SearchType==SearchType.Spouse&&args.TargetPeople.Gender==GenderType.Male))
                {
                    // 过滤逻辑
                    filtered = string.IsNullOrEmpty(args.Text)
                    ? new List<People>()
                    : FemaleList.Where(p =>
                        (!string.IsNullOrEmpty(p.IdName) && p.IdName.IndexOf(args.Text, StringComparison.OrdinalIgnoreCase) >= 0) ||
                        (!string.IsNullOrEmpty(p.Name) && p.Name.IndexOf(args.Text, StringComparison.OrdinalIgnoreCase) >= 0) ||
                        (!string.IsNullOrEmpty(p.Dynasty) && p.Dynasty.IndexOf(args.Text, StringComparison.OrdinalIgnoreCase) >= 0)
                    ).ToList();
                }
                else
                {
                    // 过滤逻辑
                    filtered = string.IsNullOrEmpty(args.Text)
                    ? new List<People>()
                    : MaleList.Where(p =>
                        (!string.IsNullOrEmpty(p.IdName) && p.IdName.IndexOf(args.Text, StringComparison.OrdinalIgnoreCase) >= 0) ||
                        (!string.IsNullOrEmpty(p.Name) && p.Name.IndexOf(args.Text, StringComparison.OrdinalIgnoreCase) >= 0) ||
                        (!string.IsNullOrEmpty(p.Dynasty) && p.Dynasty.IndexOf(args.Text, StringComparison.OrdinalIgnoreCase) >= 0)
                    ).ToList();
                }


                Suggestions.Clear();
                foreach (var person in filtered)
                {
                    Suggestions.Add(person);
                }
            }
        }

        public void ChangeParent(People child, People parent, SearchType isMom)
        {
            if (child == null || parent == null)
            {
                return;
            }

            if (isMom== SearchType.Mom)
            {
                if (child.Mom != null)
                {
                    child.Mom.Children.Remove(child);
                }
                child.Mom = parent;
                parent.Children.Add(child);
            }
            else
            {
                if (child.Dad != null)
                {
                    child.Dad.Children.Remove(child);
                }
                child.Dad = parent;
                parent.Children.Add(child);

            }
        }

        public void ChangeSpouse(People target, People spouse)
        {
            LifeEvent targetOldMarriage = target.LifeEventList.FirstOrDefault(e => e.EventType == LifeEventType.Marriage);
            if (target == null || spouse == null|| targetOldMarriage==null)
            {
                return;
            }

            if (target.Spouse != null)
            {
                target.Spouse.LifeEventList.Remove(target.Spouse.LifeEventList.FirstOrDefault(e => e.EventType == LifeEventType.Marriage));
                target.Spouse.Spouse = null;
            }
            target.Spouse = spouse;            

            LifeEvent spouseOldMarriage = spouse.LifeEventList.FirstOrDefault(e => e.EventType == LifeEventType.Marriage);
            if (spouseOldMarriage != null)
            {
                spouse.LifeEventList.Remove(spouseOldMarriage);               
            }
            spouse.LifeEventList.Add(targetOldMarriage);

            if (spouse.Spouse != null)
            {
                spouse.Spouse.LifeEventList.Remove(spouse.Spouse.LifeEventList.FirstOrDefault(e => e.EventType == LifeEventType.Marriage));
                spouse.Spouse.Spouse = null;
            }
            spouse.Spouse = target;
        }

        public void HandleQuerySubmitted(QuerySubmittedEventArgs querySubmittedEvent)
        {
            if (querySubmittedEvent.ChosenSuggestion != null && querySubmittedEvent.ChosenSuggestion is People person)
            {
                if(querySubmittedEvent.SearchType != SearchType.Spouse)
                {
                    ChangeParent(querySubmittedEvent.TargetPeople, person, querySubmittedEvent.SearchType);
                }
                else
                {
                    ChangeSpouse(querySubmittedEvent.TargetPeople, person);
                }
            }
            else
            {
                People searchResult;
                UpdateFemaleAndMaleList();
                if (querySubmittedEvent.SearchType == SearchType.Mom || (querySubmittedEvent.SearchType == SearchType.Spouse && querySubmittedEvent.TargetPeople.Gender == GenderType.Male))
                {
                    searchResult = FemaleList.FirstOrDefault(p => p.IdName == querySubmittedEvent.QueryText);
                }
                else
                {
                    searchResult = MaleList.FirstOrDefault(p => p.IdName == querySubmittedEvent.QueryText);
                }

                if (querySubmittedEvent.SearchType != SearchType.Spouse)
                {
                    ChangeParent(querySubmittedEvent.TargetPeople, searchResult, querySubmittedEvent.SearchType);
                }
                else
                {
                    ChangeSpouse(querySubmittedEvent.TargetPeople, searchResult);
                }
                
            }
        }

        public void HandleSuggestionChosen(object selectedItem)
        {
            if (selectedItem is People person)
            {
                SearchText = person.IdName;
            }
        }

        private void GotoFileSetting()
        {
            // 获取该区域
            IRegion region = _regionManager.Regions["ContentRegion"];
            IRegion region2 = _regionManager.Regions["MiddleRegion"];
            foreach (var view in region2.Views)
            {
                region2.Remove(view);
            }

            // 查找通过类型名注册的视图
            var viewInstance = region.Views.FirstOrDefault(v => v.GetType().Name == nameof(FileReadWriteUserControl));
            // 예果找까지了视图，则将其부터区域中移除
            if (viewInstance == null)
            {
                if (FileReadWrite == null)
                {
                    return;
                }
                var parameters = new NavigationParameters();
                parameters.Add("FileReadWrite", FileReadWrite);
                _regionManager.RequestNavigate("ContentRegion", "FileReadWrite", parameters);
            }
            else
            {
                foreach (var view in region.Views)
                {
                    region.Remove(view);
                }
            }

        }
        private void GotoDatabase()
        {
            // 获取该区域

            IRegion region = _regionManager.Regions["ContentRegion"];
            var viewInstance = region.Views.FirstOrDefault(v => v.GetType().Name == nameof(DatabaseUserControl));
            if (viewInstance == null)
            {
                var parameters = new NavigationParameters();
                parameters.Add("FamilyList", FamilyList);
                parameters.Add("Counties", Counties);
                _regionManager.RequestNavigate("ContentRegion", "Database", parameters);
            }
            else
            {
                foreach (var view in region.Views)
                {
                    region.Remove(view);
                }
            }


        }
        private void GotoLandedTitles()
        {
            IRegion region = _regionManager.Regions["ContentRegion"];
            IRegion region2 = _regionManager.Regions["MiddleRegion"];
            foreach (var view in region2.Views) region2.Remove(view);
            var viewInstance = region.Views.FirstOrDefault(v => v.GetType().Name == "LandedTitlesUserControl");
            if (viewInstance == null)
            {
                _regionManager.RequestNavigate("ContentRegion", "LandedTitles");
            }
            else
            {
                foreach (var view in region.Views) region.Remove(view);
            }
        }

        private void GotoStatistics()
        {
            // 获取该区域

            IRegion region = _regionManager.Regions["ContentRegion"];
            var viewInstance = region.Views.FirstOrDefault(v => v.GetType().Name == nameof(StatisticsUserControl));
            if (viewInstance == null)
            {
                var parameters = new NavigationParameters();
                parameters.Add("FamilyList", FamilyList);
                parameters.Add("Counties", Counties);
                _regionManager.RequestNavigate("ContentRegion", "Statistics", parameters);
            }
            else
            {
                foreach (var view in region.Views)
                {
                    region.Remove(view);
                }
            }
            

        }
        private void GotoTimeline()
        {
            if (SelectFamily == null)
            {
                HandyControl.Controls.Growl.Error("请先선택一个家族");
                return;
            }
            // 获取该区域
            IRegion region = _regionManager.Regions["ContentRegion"];
            foreach (var view in region.Views)
            {
                region.Remove(view);
            }
            region = _regionManager.Regions["MiddleRegion"];

            // 查找通过类型名注册的视图
            var viewInstance = region.Views.FirstOrDefault(v => v.GetType().Name == nameof(CountyTimelineUserControl));
            // 예果找까지了视图，则将其부터区域中移除
            if (viewInstance == null)
            {
                if (FileReadWrite == null)
                {
                    return;
                }
                var parameters = new NavigationParameters();
                parameters.Add("FileReadWrite", FileReadWrite);
                parameters.Add("FamilyList", FamilyList);
                parameters.Add("SelectFamily", SelectFamily);
                parameters.Add("Counties", Counties);
                _regionManager.RequestNavigate("MiddleRegion", "CountyTimeline", parameters);

                region = _regionManager.Regions["RightRegion"];
                viewInstance = region.Views.FirstOrDefault(v => v.GetType().Name == nameof(GenealogyUserControl));
                if (viewInstance == null&&SelectFamily!=null)
                {
                   
                    parameters = new NavigationParameters();
                    parameters.Add("RootFamily", SelectFamily);
                    parameters.Add("CultureOptions", CultureOptions);
                    parameters.Add("ReligionOptions", ReligionOptions);
                    parameters.Add("Suggestions", Suggestions);
                    parameters.Add("RandomNameList", RandomNameList);
                    parameters.Add("FileReadWrite", FileReadWrite);
                    _regionManager.RequestNavigate("RightRegion", "Genealogy", parameters);
                }
            }
            else
            {
                foreach (var view in region.Views)
                {
                    region.Remove(view);
                }
                region = _regionManager.Regions["RightRegion"];
                foreach (var view in region.Views)
                {
                    region.Remove(view);
                }
            }

        }

        private void GotoCoa()
        {
            try
            {
                Process coaProcess = new Process();
                coaProcess.StartInfo.FileName = FileReadWrite.CoaCollectSoftwarePath;

                // 启动进程
                coaProcess.Start();

                // 等待进程完全启动
                coaProcess.WaitForInputIdle(10000); // 等待最多10秒钟
            }
            catch (Exception ex)
            {
                MessageBox.Show($"문장 수집 프로그램을 여는 중 예외가 발생했습니다. 원인: {ex.Message}");
            }

        }

        private void GotoFileContent()
        {
            // 获取该区域
            IRegion region = _regionManager.Regions["FileContentRegion"];
            IRegion region2 = _regionManager.Regions["MiddleRegion"];
            foreach (var view in region2.Views)
            {
                region2.Remove(view);
            }

            // 查找通过类型名注册的视图
            var viewInstance = region.Views.FirstOrDefault(v => v.GetType().Name == nameof(FileContentUserControl));
            // 예果找까지了视图，则将其부터区域中移除
            if (viewInstance == null)
            {
                var parameters = new NavigationParameters();
                parameters.Add("FileReadWrite", FileReadWrite);
                parameters.Add("FamilyList", FamilyList);
                _regionManager.RequestNavigate("FileContentRegion", "FileContent", parameters);
            }
            else
            {
                foreach (var view in region.Views)
                {
                    region.Remove(view);
                }
            }

        }

        private void FullScreen()
        {
            // 获取该区域
            IRegion region = _regionManager.Regions["ContentRegion"];
            IRegion region2 = _regionManager.Regions["MiddleRegion"];
            IRegion region3 = _regionManager.Regions["RightRegion"];

            var viewInstance = region.Views.FirstOrDefault(v => v.GetType().Name == nameof(GenealogyUserControl));
            if (viewInstance != null)
            {
                foreach (var view in region.Views)
                {
                    region.Remove(view);
                }

                return;
            }

            viewInstance = region3.Views.FirstOrDefault(v => v.GetType().Name == nameof(GenealogyUserControl));
            if (viewInstance != null)
            {
                foreach (var view in region3.Views)
                {
                    region3.Remove(view);
                }

                return;
            }

            if (SelectFamily == null)
            {
                HandyControl.Controls.Growl.Error("请先선택一个家族");
                return;
            }

            var parameters = new NavigationParameters();
            parameters.Add("RootFamily", SelectFamily);
            parameters.Add("CultureOptions", CultureOptions);
            parameters.Add("ReligionOptions", ReligionOptions);
            parameters.Add("Suggestions", Suggestions);
            parameters.Add("RandomNameList", RandomNameList);
            parameters.Add("FileReadWrite", FileReadWrite);

            viewInstance = region2.Views.FirstOrDefault(v => v.GetType().Name == nameof(CountyTimelineUserControl));
            if (viewInstance != null)
            {
                _regionManager.RequestNavigate("RightRegion", "Genealogy", parameters);
            }
            else
            {
                _regionManager.RequestNavigate("ContentRegion", "Genealogy", parameters);
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
        public void UpdateFemaleAndMaleList()
        {
            if (FamilyList == null)
            {
                return;
            }

            // 创建一个临时集合来存储所有应该在列表中的项目
            HashSet<People> shouldBeInFemaleList = new HashSet<People> ();
            HashSet<People> shouldBeInMaleList = new HashSet<People> ();

            if (SelectPeople != null)
            {
                LifeEvent selPeopleBirthEvent = SelectPeople.LifeEventList.FirstOrDefault(e => e.EventType == LifeEventType.Birth);
                LifeEvent selPeopleDeathEvent = SelectPeople.LifeEventList.FirstOrDefault(e => e.EventType == LifeEventType.Death);

                if (selPeopleBirthEvent != null || selPeopleDeathEvent != null)
                {
                    // 填充这些集合
                    foreach (var family in FamilyList)
                    {
                        foreach (var person in family.Members)
                        {
                            LifeEvent tmpPeopleBirthEvent = person.LifeEventList.FirstOrDefault(e => e.EventType == LifeEventType.Birth);
                            LifeEvent tmpPeopleDeathEvent = person.LifeEventList.FirstOrDefault(e => e.EventType == LifeEventType.Death);
                            if(tmpPeopleBirthEvent != null && selPeopleBirthEvent != null && tmpPeopleBirthEvent > selPeopleBirthEvent)
                            {
                                continue;
                            }
                            else if(tmpPeopleDeathEvent != null && selPeopleBirthEvent != null && tmpPeopleDeathEvent < selPeopleBirthEvent)
                            {
                                continue;
                            }
                            if (person.Gender == GenderType.Female)
                            {
                                shouldBeInFemaleList.Add(person);
                            }
                            else if (person.Gender == GenderType.Male)
                            {
                                shouldBeInMaleList.Add(person);
                            }
                        }
                    }
                }
                else
                {
                    // 填充这些集合
                    foreach (var family in FamilyList)
                    {
                        foreach (var person in family.Members)
                        {

                            if (person.Gender == GenderType.Female)
                            {
                                shouldBeInFemaleList.Add(person);
                            }
                            else if (person.Gender == GenderType.Male)
                            {
                                shouldBeInMaleList.Add(person);
                            }
                        }
                    }
                }
            }
            else
            {
                // 填充这些集合
                foreach (var family in FamilyList)
                {
                    foreach (var person in family.Members)
                    {

                        if (person.Gender == GenderType.Female)
                        {
                            shouldBeInFemaleList.Add(person);
                        }
                        else if (person.Gender == GenderType.Male)
                        {
                            shouldBeInMaleList.Add(person);
                        }
                    }
                }
            }

            

            

            // 예果有选中的人员，将其부터对应列表中移除
            if (SelectPeople != null)
            {
                if (SelectPeople.Gender == GenderType.Female)
                {
                    shouldBeInFemaleList.Remove(SelectPeople);
                }
                else
                {
                    shouldBeInMaleList.Remove(SelectPeople);
                }
            }

            // 移除不应该在列表中的项目
            for (int i = FemaleList.Count - 1; i >= 0; i--)
            {
                if (!shouldBeInFemaleList.Contains(FemaleList[i]))
                {
                    FemaleList.RemoveAt(i);
                }
            }

            for (int i = MaleList.Count - 1; i >= 0; i--)
            {
                if (!shouldBeInMaleList.Contains(MaleList[i]))
                {
                    MaleList.RemoveAt(i);
                }
            }

            // 添加应该在列表中但尚未添加的项目
            foreach (var femaleName in shouldBeInFemaleList)
            {
                if (!FemaleList.Contains(femaleName))
                {
                    FemaleList.Add(femaleName);
                }
            }

            foreach (var maleName in shouldBeInMaleList)
            {
                if (!MaleList.Contains(maleName))
                {
                    MaleList.Add(maleName);
                }
            }

            
        }
        private void DelLifeEvent()
        {
            if (SelectPeople == null||SelectLifeEvent==null)
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
            bool confirmed = false;

            DialogParameters parm = new DialogParameters
    {
        { "value", tmpLifeEvent }
    };
            _dialogService.ShowDialog(nameof(CommonAssignmentWindow), parm, arg =>
            {
                if (arg.Result == ButtonResult.OK)
                {
                    tmpLifeEvent = arg.Parameters.GetValue<LifeEvent>("value");
                    confirmed = true;
                }
            });

            if (confirmed)
            {
                SelectPeople.LifeEventList.Add(tmpLifeEvent);
            }
        }
        private void OutputAllFamilyFile()
        {
            // 创建一个 SaveFileDialog 实例
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",  // 설정파일类型过滤
                FileName = $"AllFamily"  // 설정默认파일名
            };

            // 열기파일선택框，用户선택경로
            bool? result = saveFileDialog.ShowDialog();

            if (result == true) // 예果用户点击了“저장”
            {
                string filePath = saveFileDialog.FileName; // 获取선택的파일경로

                try
                {
                    // 使用 StreamWriter 기록파일内容
                    using (StreamWriter writer = new StreamWriter(filePath))
                    {
                        string content = "";
                        foreach (var family in FamilyList)
                        {
                            content += family.ToString();
                            content += "\r\n";
                        }
                        writer.Write(content); // 将传入的 content 기록파일
                    }

                    MessageBox.Show("파일이 저장되었습니다", "성공", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"파일 저장 중 오류: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void OutputFamilyFile()
        {
            if (SelectFamily == null)
            {
                return;
            }
            // 创建一个 SaveFileDialog 实例
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",  // 설정파일类型过滤
                FileName = $"{SelectFamily.FamilyName}"  // 설정默认파일名
            };

            // 열기파일선택框，用户선택경로
            bool? result = saveFileDialog.ShowDialog();

            if (result == true) // 예果用户点击了“저장”
            {
                string filePath = saveFileDialog.FileName; // 获取선택的파일경로

                try
                {
                    // 使用 StreamWriter 기록파일内容
                    using (StreamWriter writer = new StreamWriter(filePath))
                    {
                        writer.Write(SelectFamily.ToString()); // 将传入的 content 기록파일
                    }

                    MessageBox.Show("파일이 저장되었습니다", "성공", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"파일 저장 중 오류: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void OutputPersonFile()
        {
            if (SelectPeople == null)
            {
                return;
            }
            // 创建一个 SaveFileDialog 实例
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",  // 설정파일类型过滤
                FileName = $"{SelectPeople.IdName}" // 설정默认파일名
            };

            // 열기파일선택框，用户선택경로
            bool? result = saveFileDialog.ShowDialog();

            if (result == true) // 예果用户点击了“저장”
            {
                string filePath = saveFileDialog.FileName; // 获取선택的파일경로

                try
                {
                    // 使用 StreamWriter 기록파일内容
                    using (StreamWriter writer = new StreamWriter(filePath))
                    {
                        writer.Write(SelectPeople.GetString()); // 将传入的 content 기록파일
                    }

                    MessageBox.Show("파일이 저장되었습니다", "성공", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"파일 저장 중 오류: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void PopulateReligionOptions()
        {
            var religionSet = new HashSet<string>(); // 用于存储唯一的종교名称

            // 遍历 FamilyList 中的每个 Family
            foreach (var family in FamilyList)
            {
                // 遍历每个 Family 中的 Members
                foreach (var member in family.Members)
                {
                    // 예果 Religion 不为空并且不重复，添加까지 religionSet 中
                    if (!string.IsNullOrEmpty(member.Religion))
                    {
                        religionSet.Add(member.Religion);
                    }
                }
            }

            // 将 HashSet 中的종교名称添加까지 ReligionOptions 中
            ReligionOptions.Clear(); // 비우기现有的종교选项
            foreach (var religion in religionSet)
            {
                ReligionOptions.Add(religion);
            }
        }

        private void PopulateCultureOptions()
        {
            var religionSet = new HashSet<string>(); // 用于存储唯一的종교名称

            // 遍历 FamilyList 中的每个 Family
            foreach (var family in FamilyList)
            {
                // 遍历每个 Family 中的 Members
                foreach (var member in family.Members)
                {
                    // 예果 Religion 不为空并且不重复，添加까지 religionSet 中
                    if (!string.IsNullOrEmpty(member.Culture))
                    {
                        religionSet.Add(member.Culture);
                    }
                }
            }

            foreach (var item in RandomNameList)
            {
                // 예果 Religion 不为空并且不重复，添加까지 religionSet 中
                if (!string.IsNullOrEmpty(item.CultureName))
                {
                    religionSet.Add(item.CultureName);
                }
            }

            // 将 HashSet 中的종교名称添加까지 ReligionOptions 中
            CultureOptions.Clear(); // 비우기现有的종교选项
            foreach (var religion in religionSet)
            {
                CultureOptions.Add(religion);
            }
        }

        private void AddReligionExecute()
        {
            dynamic obj = new ExpandoObject();
            obj.종교名 = "";
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
            if (!ReligionOptions.Contains(obj.종교名) && !string.IsNullOrWhiteSpace(obj.종교名))
            {
                ReligionOptions.Add(obj.종교名);  // 예果不存在，则添加
            }
        }
        private void AddCultureExecute()
        {
            dynamic obj = new ExpandoObject();
            obj.문화名 = "";
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
            // 判断CultureOptions中是否已经存在相同的문화名
            if (!CultureOptions.Contains(obj.문화名) && !string.IsNullOrWhiteSpace(obj.문화名))
            {
                CultureOptions.Add(obj.문화名);  // 예果不存在，则添加
            }

        }
        private void CloseCommandExecute()
        {
            _eventAggregator.GetEvent<SaveMessageEvent>().Publish();
        }

        public void GetFamilyForSelectedPeople()
        {
            // 遍历所有的 Family
            foreach (var family in FamilyList)
            {
                // 예果当前 Family 的 Members 包含 SelectPeople，返回该 Family
                if (family.Members.Contains(SelectPeople))
                {
                    SelectFamily=family;
                    return;
                }
            }
        }
        private void Load()
        {
            try
            {
                FileReadWrite = JsonHelper.LoadFileSetting();
                FileReadWrite.PropertyChanged += (s, e) => {
                    _eventAggregator.GetEvent<FileSettingChangeEvent>().Publish(e.PropertyName);
                };

                FamilyList = JsonHelper.LoadData();
                

                RandomNameList = NameFileParser.ParseCultures(FileReadWrite.RandomNameFilePath);
                CountyParser.ParseCountiesFromFile(Counties, FileReadWrite.DomainDefFile);

                EventBindingAllFamily();
                PopulateReligionOptions();
                PopulateCultureOptions();
                InitPipeService();
            }
            catch (Exception ex)
            {
                // 处理오류
                //MessageBox.Show($"가문 데이터 로드 실패: {ex.Message}");
            }
        }

        private void InitPipeService()
        {
            PipeClientService.StartPolling();
            PipeClientService.Connected += () =>
            {
                IsPipeServiceConnect = true;
                HandyControl.Controls.Growl.Success("已连接까지服务器");
            };
            PipeClientService.Disconnected += () =>
            {
                IsPipeServiceConnect = false;
                HandyControl.Controls.Growl.Warning("与服务器断开连接");
            };
        }

        private void EventBindingAllFamily()
        {
            // 遍历 FamilyList 中的每个 Family
            foreach (var family in FamilyList)
            {
                EventBindingFamily(family);
            }

            FamilyList.CollectionChanged += (s, e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    
                    foreach (Family family in e.NewItems)
                    {
                        _eventAggregator.GetEvent<UpdateContentEvent>().Publish(new UpdateContentEventArgs() { type = UpdateContentType.UpdateSingleFamily, value = family });
                        EventBindingFamily(family);
                    }
                }
                else if (e.Action == NotifyCollectionChangedAction.Remove)
                {
                    foreach (Family family in e.OldItems)
                    {
                        _eventAggregator.GetEvent<UpdateContentEvent>().Publish(new UpdateContentEventArgs() { type = UpdateContentType.RemoveSingleFamily, value = family });
                        EventUnBindingFamily(family);
                    }
                }
                
            };
            
        }

        private void EventBindingFamily(Family family)
        {
            family.Init();
            // 遍历每个 Family 中的 Members
            foreach (var people in family.Members)
            {
                people.Init();
                people.PropertyChanged += (s, e) =>
                {
                    _eventAggregator.GetEvent<UpdateContentEvent>().Publish(new UpdateContentEventArgs() { type = UpdateContentType.UpdateSingleCharacter, value = people });
                };
            }

            family.Members.CollectionChanged += (s, e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    foreach (People newChild in e.NewItems)
                    {
                        _eventAggregator.GetEvent<UpdateContentEvent>().Publish(new UpdateContentEventArgs() { type = UpdateContentType.UpdateSingleCharacter, value = newChild });
                        newChild.PropertyChanged += (s2, e2) =>
                        {
                            _eventAggregator.GetEvent<UpdateContentEvent>().Publish(new UpdateContentEventArgs() { type = UpdateContentType.UpdateSingleCharacter, value = newChild });
                        };
                    }
                }
                else if (e.Action == NotifyCollectionChangedAction.Remove)
                {
                    foreach (People oldChild in e.OldItems)
                    {
                        _eventAggregator.GetEvent<UpdateContentEvent>().Publish(new UpdateContentEventArgs() { type = UpdateContentType.RemoveSingleCharacter, value = oldChild });
                        oldChild.PropertyChanged -= (s2, e2) =>
                        {
                            _eventAggregator.GetEvent<UpdateContentEvent>().Publish(new UpdateContentEventArgs() { type = UpdateContentType.UpdateSingleCharacter, value = oldChild });
                        };
                    }
                }
               
            };          
        }


        private void EventUnBindingFamily(Family family)
        {
            // 遍历每个 Family 中的 Members
            foreach (var people in family.Members)
            {
                _eventAggregator.GetEvent<UpdateContentEvent>().Publish(new UpdateContentEventArgs() { type = UpdateContentType.RemoveSingleCharacter, value = people });
                people.PropertyChanged -= (s, e) =>
                {
                    _eventAggregator.GetEvent<UpdateContentEvent>().Publish(new UpdateContentEventArgs() { type = UpdateContentType.UpdateSingleCharacter, value = people });
                };
            }

            family.Members.CollectionChanged -= (s, e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    foreach (People newChild in e.NewItems)
                    {
                        newChild.PropertyChanged += (s2, e2) =>
                        {
                            _eventAggregator.GetEvent<UpdateContentEvent>().Publish(new UpdateContentEventArgs() { type = UpdateContentType.UpdateSingleCharacter, value = newChild });
                        };
                    }
                }
                else if (e.Action == NotifyCollectionChangedAction.Remove)
                {
                    foreach (People oldChild in e.OldItems)
                    {
                        oldChild.PropertyChanged -= (s2, e2) =>
                        {
                            _eventAggregator.GetEvent<UpdateContentEvent>().Publish(new UpdateContentEventArgs() { type = UpdateContentType.UpdateSingleCharacter, value = oldChild });
                        };
                    }
                }
                
            };
        }

        // 열기파일的逻辑
        private void OpenFile()
        {
            _dialogService.ShowDialog(nameof(FamilyTreeWindow), arg => { });
        }

        private void AddPeople()
        {
            if (SelectFamily == null) return;

            dynamic obj = new ExpandoObject();
            obj.性别 = GenderType.Male;
            obj.名字 = "";
            bool confirmed = false;

            DialogParameters parm = new DialogParameters
        {
            { "value", obj }
        };
            _dialogService.ShowDialog(nameof(AssignmentWindow), parm, arg =>
            {
                if (arg.Result == ButtonResult.OK)
                {
                    obj = arg.Parameters.GetValue<ExpandoObject>("value");
                    confirmed = true;
                }
            });

            if (confirmed)
            {
                string religion = ReligionOptions.LastOrDefault();
                string culture = CultureOptions.LastOrDefault();
                SelectFamily.Members.Add(new People(SelectFamily.FindMemberWithMaxIdNumber() + 1, obj.名字, obj.性别, SelectFamily.FamilyName, religion, culture));
            }
        }
        private void DelPeople()
        {
            if (SelectFamily!=null&&SelectPeople!=null)
            {
                
                SelectFamily.Members.Remove(SelectPeople);
                if(SelectPeople.Mom != null)
                {
                    SelectPeople.Mom.Children.Remove(SelectPeople);
                }
                if(SelectPeople.Dad != null)
                {
                    SelectPeople.Dad.Children.Remove(SelectPeople);
                }
                if(SelectPeople.Spouse != null)
                {
                    SelectPeople.Spouse.LifeEventList.Remove(SelectPeople.Spouse.LifeEventList.FirstOrDefault(e => e.EventType == LifeEventType.Marriage));
                    SelectPeople.Spouse.Spouse = null;
                }
            }
            
        }
        private void AddFamily()
        {
            dynamic obj = new ExpandoObject();
            obj.英文家族名 = "";
            obj.中文家族名 = "";
            bool confirmed = false;

            DialogParameters parm = new DialogParameters
        {
            { "value", obj }
        };
            _dialogService.ShowDialog(nameof(AssignmentWindow), parm, arg =>
            {
                if (arg.Result == ButtonResult.OK)
                {
                    obj = arg.Parameters.GetValue<ExpandoObject>("value");
                    confirmed = true;
                }
            });

            if (confirmed)
            {
                FamilyList.Add(new Family(obj.英文家族名, obj.中文家族名));
            }
        }
        private void DelFamily()
        {
            if (SelectFamily != null)
            {
                FamilyList.Remove(SelectFamily);
            }
            
        }

        private void CreatePersonContent()
        {
            People oldSelectPeople = SelectPeople;
            SelectPeople = new People();
            SelectPeople = oldSelectPeople;
        }

        private void CreateFamilyContent()
        {
            Family oldFamily= SelectFamily;
            SelectFamily = new Family();
            SelectFamily = oldFamily;
        }

        private void CreateAllFamilyContent()
        {
            ObservableCollection<Family> oldfamilyList = FamilyList;
            FamilyList = new ObservableCollection<Family>();
            FamilyList=oldfamilyList;
        }

        // IDestructible 接口实现，在 ViewModel 销毁时调用
        public void Destroy()
        {
            try
            {
                // 因为是在销毁时调用，使用同步方式
                Task.Run(async () => await _repository.SaveFamiliesAsync(FamilyList)).Wait();
            }
            catch (Exception ex)
            {
                // 记录日志
            }
        }

        private void Save()
        {
            try
            {
                // 因为是在销毁时调用，使用同步方式
                //Task.Run(async () => await _repository.SaveFamiliesAsync(FamilyList)).Wait();
                JsonHelper.SaveData(FamilyList);
                CountyParser.SaveCountiesToFile(Counties, FileReadWrite.DomainDefFile);
                JsonHelper.SaveFileSetting(FileReadWrite);
                HandyControl.Controls.Growl.Success("저장파일성공");
            }
            catch (Exception ex)
            {
                HandyControl.Controls.Growl.Error($@"저장파일失败，原因是{ex.Message}");
            }
        }

    }
}
