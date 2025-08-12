using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Web.UI.WebControls;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using WpfPrismFrameworkTemplate.Helper;
using WpfPrismFrameworkTemplate.Model;

namespace WpfPrismFrameworkTemplate.ViewModels
{
    public class DatabaseUserControlViewModel : BindableBase, INavigationAware
    {
        private ObservableCollection<County> _Counties = new ObservableCollection<County>();
        private ObservableCollection<Family> _FamilyList = new ObservableCollection<Family>();       
        private readonly SqlServerDatabaseHelper _SqlServerDbHelper;
        private IEventAggregator _eventAggregator;
        private FlowDocument _DebugMsg= new FlowDocument();
        private int _currentTabIndex;
        public DelegateCommand TestCmd { get; private set; }
        public DelegateCommand ClearDebugMsgCmd { get; private set; }

        public DatabaseUserControlViewModel(SqlServerDatabaseHelper sqlServerDbHelper, IEventAggregator eventAggregator)
        {          
            _SqlServerDbHelper = sqlServerDbHelper;
            _eventAggregator = eventAggregator;
            _eventAggregator.GetEvent<DebugMsgEvent>().Subscribe(ShowDebugMsg);
            TestCmd = new DelegateCommand(Test);
            ClearDebugMsgCmd = new DelegateCommand(ClearDebugMsg);
        }
        
        public int CurrentTabIndex
        {
            get => _currentTabIndex;
            set
            {
                if (_currentTabIndex != value)
                {
                    SetProperty(ref _currentTabIndex, value);
                }
            }
        }
        public FlowDocument DebugMsg
        {
            get => _DebugMsg;
            set
            {
                SetProperty(ref _DebugMsg, value);
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
        public ObservableCollection<County> Counties
        {
            get => _Counties;
            set => SetProperty(ref _Counties, value);
        }

        public void ClearDebugMsg()
        {
            try
            {
                DebugMsg.Blocks.Clear();
            }
            catch (Exception ex)
            {
                DebugHelper.Instance.Log($@"{ex.Message}", MsgLevel.Alarm);
            }
        }
        public void Test()
        {
            try
            {
                switch (CurrentTabIndex)
                {
                    case 0:
                        using (var dbHelper = new SqliteDatabaseHelper(@"D:\sqlite_database\test3.db"))
                        {
                            // 测试连接
                            if (dbHelper.TestConnection())
                            {
                                DebugHelper.Instance.Log("数据库连接成功", MsgLevel.Success);
                            }
                        }
                        break;
                    case 1: 
                        
                        break;
                    case 2:
                        if (_SqlServerDbHelper.TestConnection())
                        {
                            DebugHelper.Instance.Log("SQL Server数据库连接成功", MsgLevel.Success);
                        }
                        else
                        {
                            DebugHelper.Instance.Log("SQL Server数据库连接失败", MsgLevel.Warning);
                        }
                        break;
                    default:
                        break;

                }
                
            }
            catch (Exception ex)
            {
                DebugHelper.Instance.Log($@"{ex.Message}",MsgLevel.Alarm);
            }
        }

        public void ShowDebugMsg(DebugMsgEventArgs args)
        {
            try
            {
                Paragraph paragraph = new Paragraph();
                Run run = new Run($@"{args.timestamp}:{args.content}");
                switch(args.level)
                {
                    case MsgLevel.Normal:
                        run.Foreground = Brushes.Black;
                        break;
                    case MsgLevel.Warning:
                        run.Foreground = Brushes.Orange;
                        break;
                    case MsgLevel.Alarm:
                        run.Foreground = Brushes.Red;
                        break;
                    case MsgLevel.Success:
                        run.Foreground = Brushes.Green;
                        break;
                    default:
                        run.Foreground = Brushes.Black;
                        break;
                }

                paragraph.Inlines.Add(run);
                DebugMsg.Blocks.Add(paragraph);
            }
            catch (Exception ex)
            {
                Console.WriteLine($@"{ex.Message}");
            }
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (navigationContext.Parameters.ContainsKey("FamilyList")
                && navigationContext.Parameters.ContainsKey("Counties"))
            {
                FamilyList = navigationContext.Parameters.GetValue<ObservableCollection<Family>>("FamilyList");
                Counties = navigationContext.Parameters.GetValue<ObservableCollection<County>>("Counties");
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
