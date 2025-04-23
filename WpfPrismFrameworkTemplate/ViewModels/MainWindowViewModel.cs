using Google.Protobuf.Compiler;
using Microsoft.Win32;
using ModernWpf.Controls;
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
using System.Diagnostics.Metrics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Linq;
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
        private People _SelectPeople;
        private Family _SelectFamily;
        private string _title = "CK3创建人物工具(作者:Fred)";
        private string _FileContent = "";
        private string _HighlightedContent = "";
        private object _selectedItem;
        private readonly FamilyRepository _repository;
        private IEventAggregator _eventAggregator;
        private bool _EnableDatabaseSync = false;
        private IDialogService _dialogService;
        private LifeEvent _SelectLifeEvent=new LifeEvent();
        private readonly IRegionManager _regionManager;
        private string _searchText;

        private ObservableCollection<People> _TestPeopleList = new ObservableCollection<People>();
        public ObservableCollection<People> TestPeopleList
        {
            get => _TestPeopleList;
            set => SetProperty(ref _TestPeopleList, value);
        }


        public DelegateCommand OpenFileCmd { get; private set; }
        public DelegateCommand<string> SearchCmd { get; private set; }
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
        public MainWindowViewModel(IEventAggregator eventAggregator, IDialogService dialogService, IRegionManager regionManager)
		{
            _eventAggregator = eventAggregator;
            _dialogService = dialogService;
            _regionManager = regionManager;
            


            _repository = new FamilyRepository(eventAggregator);

            OpenFileCmd = new DelegateCommand(OpenFile);
            SearchCmd = new DelegateCommand<string>(SearchContent);
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
            eventAggregator.GetEvent<SaveMessageEvent>().Subscribe(Save);

            LoadFamilies();

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
                LoadFamilies();
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
            set => SetProperty(ref _SelectFamily, value);
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
        public string HighlightedContent
        {
            get => _HighlightedContent;
            set => SetProperty(ref _HighlightedContent, value);
        }
        public string FileContent
        {
            get { return _FileContent; }
            set { SetProperty(ref _FileContent, value); }
        }
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        public void HandleTextChanged(string text, AutoSuggestionBoxTextChangeReason reason,bool isMom=true)
        {
            if (reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                // 保存搜索文本
                SearchText = text;
                List<People> filtered;
                UpdateFemaleAndMaleList();

                if (isMom)
                {
                    // 过滤逻辑
                    filtered = string.IsNullOrEmpty(text)
                    ? new List<People>()
                    : FemaleList.Where(p =>
                        (!string.IsNullOrEmpty(p.IdName) && p.IdName.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0) ||
                        (!string.IsNullOrEmpty(p.Name) && p.Name.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0) ||
                        (!string.IsNullOrEmpty(p.Dynasty) && p.Dynasty.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0)
                    ).ToList();
                }
                else
                {
                    // 过滤逻辑
                    filtered = string.IsNullOrEmpty(text)
                    ? new List<People>()
                    : MaleList.Where(p =>
                        (!string.IsNullOrEmpty(p.IdName) && p.IdName.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0) ||
                        (!string.IsNullOrEmpty(p.Name) && p.Name.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0) ||
                        (!string.IsNullOrEmpty(p.Dynasty) && p.Dynasty.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0)
                    ).ToList();
                }

                Suggestions.Clear();
                foreach (var person in filtered)
                {
                    Suggestions.Add(person);
                }
            }
        }

        public void HandleQuerySubmitted(string queryText, object chosenSuggestion, bool isMom = true)
        {
            if (chosenSuggestion != null && chosenSuggestion is People person)
            {
                if (isMom)
                {
                    SelectPeople.Mom = person;                    
                }
                else
                {
                    SelectPeople.Dad = person;
                }
                person.Children.Add(SelectPeople);
            }
            else
            {
                UpdateFemaleAndMaleList();
                if (isMom)
                {
                    // 用户直接提交了查询文本
                    var searchResult = FemaleList.FirstOrDefault(p => p.IdName == queryText);
                    if (searchResult != null)
                    {
                        SelectPeople.Mom = searchResult;
                        searchResult.Children.Add(SelectPeople);
                    }
                }
                else
                {
                    // 用户直接提交了查询文本
                    var searchResult = MaleList.FirstOrDefault(p => p.IdName == queryText);
                    if (searchResult != null)
                    {
                        SelectPeople.Dad = searchResult;
                        searchResult.Children.Add(SelectPeople);
                    }
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

        private void FullScreen()
        {
            // 获取该区域
            IRegion region = _regionManager.Regions["ContentRegion"];

            // 查找通过类型名注册的视图
            var viewInstance = region.Views.FirstOrDefault(v => v.GetType().Name == nameof(GenealogyUserControl));

            // 如果找到了视图，则将其从区域中移除
            if (viewInstance != null)
            {
                region.Remove(viewInstance);
            }
            else
            {
                if(SelectFamily == null)
                {
                    MessageBox.Show("请先选择一个家族", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var parameters = new NavigationParameters();
                parameters.Add("RootFamily", SelectFamily);
                parameters.Add("CultureOptions", CultureOptions);
                parameters.Add("ReligionOptions", ReligionOptions);
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

                if (selPeopleBirthEvent != null || selPeopleBirthEvent != null)
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

            

            

            // 如果有选中的人员，将其从对应列表中移除
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
        private void OutputAllFamilyFile()
        {
            // 创建一个 SaveFileDialog 实例
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",  // 设置文件类型过滤
                FileName = $"AllFamily"  // 设置默认文件名
            };

            // 打开文件选择框，用户选择路径
            bool? result = saveFileDialog.ShowDialog();

            if (result == true) // 如果用户点击了“保存”
            {
                string filePath = saveFileDialog.FileName; // 获取选择的文件路径

                try
                {
                    // 使用 StreamWriter 写入文件内容
                    using (StreamWriter writer = new StreamWriter(filePath))
                    {
                        string content = "";
                        foreach (var family in FamilyList)
                        {
                            content += family.ToString();
                            content += "\r\n";
                        }
                        writer.Write(content); // 将传入的 content 写入文件
                    }

                    MessageBox.Show("文件已保存", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"保存文件时出错: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
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
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",  // 设置文件类型过滤
                FileName = $"{SelectFamily.FamilyName}"  // 设置默认文件名
            };

            // 打开文件选择框，用户选择路径
            bool? result = saveFileDialog.ShowDialog();

            if (result == true) // 如果用户点击了“保存”
            {
                string filePath = saveFileDialog.FileName; // 获取选择的文件路径

                try
                {
                    // 使用 StreamWriter 写入文件内容
                    using (StreamWriter writer = new StreamWriter(filePath))
                    {
                        writer.Write(SelectFamily.ToString()); // 将传入的 content 写入文件
                    }

                    MessageBox.Show("文件已保存", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"保存文件时出错: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
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
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",  // 设置文件类型过滤
                FileName = $"{SelectPeople.IdName}" // 设置默认文件名
            };

            // 打开文件选择框，用户选择路径
            bool? result = saveFileDialog.ShowDialog();

            if (result == true) // 如果用户点击了“保存”
            {
                string filePath = saveFileDialog.FileName; // 获取选择的文件路径

                try
                {
                    // 使用 StreamWriter 写入文件内容
                    using (StreamWriter writer = new StreamWriter(filePath))
                    {
                        writer.Write(SelectPeople.GetString()); // 将传入的 content 写入文件
                    }

                    MessageBox.Show("文件已保存", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"保存文件时出错: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void PopulateReligionOptions()
        {
            var religionSet = new HashSet<string>(); // 用于存储唯一的宗教名称

            // 遍历 FamilyList 中的每个 Family
            foreach (var family in FamilyList)
            {
                // 遍历每个 Family 中的 Members
                foreach (var member in family.Members)
                {
                    // 如果 Religion 不为空并且不重复，添加到 religionSet 中
                    if (!string.IsNullOrEmpty(member.Religion))
                    {
                        religionSet.Add(member.Religion);
                    }
                }
            }

            // 将 HashSet 中的宗教名称添加到 ReligionOptions 中
            ReligionOptions.Clear(); // 清空现有的宗教选项
            foreach (var religion in religionSet)
            {
                ReligionOptions.Add(religion);
            }
        }

        private void PopulateCultureOptions()
        {
            var religionSet = new HashSet<string>(); // 用于存储唯一的宗教名称

            // 遍历 FamilyList 中的每个 Family
            foreach (var family in FamilyList)
            {
                // 遍历每个 Family 中的 Members
                foreach (var member in family.Members)
                {
                    // 如果 Religion 不为空并且不重复，添加到 religionSet 中
                    if (!string.IsNullOrEmpty(member.Culture))
                    {
                        religionSet.Add(member.Culture);
                    }
                }
            }

            // 将 HashSet 中的宗教名称添加到 ReligionOptions 中
            CultureOptions.Clear(); // 清空现有的宗教选项
            foreach (var religion in religionSet)
            {
                CultureOptions.Add(religion);
            }
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
        private void CloseCommandExecute()
        {
            _eventAggregator.GetEvent<SaveMessageEvent>().Publish();
        }

        public void GetFamilyForSelectedPeople()
        {
            // 遍历所有的 Family
            foreach (var family in FamilyList)
            {
                // 如果当前 Family 的 Members 包含 SelectPeople，返回该 Family
                if (family.Members.Contains(SelectPeople))
                {
                    SelectFamily=family;
                    return;
                }
            }
        }
        private void LoadFamilies()
        {
            try
            {
                FamilyList = JsonHelper.LoadData();
                EventCorrelation();
                PopulateReligionOptions();
                PopulateCultureOptions();

            }
            catch (Exception ex)
            {
                // 处理错误
                //MessageBox.Show($"加载家族数据失败：{ex.Message}");
            }
        }

        private void EventCorrelation()
        {
            // 遍历 FamilyList 中的每个 Family
            foreach (var family in FamilyList)
            {
                family.Init();


                // 遍历每个 Family 中的 Members
                foreach(var people in family.Members)
                {
                    people.Init();
                }
            }
        }

        // 打开文件的逻辑
        private void OpenFile()
        {
            //// 打开文件对话框
            //var openFileDialog = new Microsoft.Win32.OpenFileDialog
            //{
            //    Title = "选择文件",
            //    Filter = "所有文件|*.*"
            //};

            //if (openFileDialog.ShowDialog() == true)
            //{
            //    FileContent = File.ReadAllText(openFileDialog.FileName);
            //    HighlightedContent = FileContent; // 默认显示为原始内容
            //}
            _dialogService.ShowDialog(nameof(FamilyTreeWindow), arg =>
            {
                if (arg.Result == ButtonResult.OK)
                {
                    
                }
            });
        }

        private void AddPeople()
        {
            if(SelectFamily!=null)
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
                SelectFamily.Members.Add(new People(SelectFamily.FindMemberWithMaxIdNumber()+1, obj.名字, obj.性别,SelectFamily.FamilyName));
            }
            
        }
        private void DelPeople()
        {
            if (SelectFamily!=null&&SelectPeople!=null)
            {
                
                SelectFamily.Members.Remove(SelectPeople);
            }
            
        }
        private void AddFamily()
        {
            dynamic obj = new ExpandoObject();
            obj.家族名 = "";
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
            var family = new Family(obj.家族名);
            FamilyList.Add(family);
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

        private void SearchContent(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm) || string.IsNullOrEmpty(FileContent))
            {
                HighlightedContent = FileContent;
                return;
            }

            // 使用正则表达式高亮匹配的内容
            var regex = new Regex(Regex.Escape(searchTerm), RegexOptions.IgnoreCase);
            HighlightedContent = regex.Replace(FileContent, match => $"<Highlight>{match.Value}</Highlight>");
        }

        private FlowDocument CreateHighlightDocument(string content, string searchTerm)
        {
            var doc = new FlowDocument();
            var paragraph = new Paragraph();

            if (string.IsNullOrEmpty(searchTerm))
            {
                paragraph.Inlines.Add(new Run(content));
            }
            else
            {
                var regex = new Regex(Regex.Escape(searchTerm), RegexOptions.IgnoreCase);
                var matches = regex.Matches(content);

                int lastIndex = 0;
                foreach (Match match in matches)
                {
                    if (match.Index > lastIndex)
                    {
                        paragraph.Inlines.Add(new Run(content.Substring(lastIndex, match.Index - lastIndex)));
                    }

                    var highlightRun = new Run(match.Value)
                    {
                        Background = Brushes.Yellow // 高亮背景
                    };
                    paragraph.Inlines.Add(highlightRun);

                    lastIndex = match.Index + match.Length;
                }

                if (lastIndex < content.Length)
                {
                    paragraph.Inlines.Add(new Run(content.Substring(lastIndex)));
                }
            }

            doc.Blocks.Add(paragraph);
            return doc;
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
            }
            catch (Exception ex)
            {
                // 记录日志
            }
        }

    }
}
