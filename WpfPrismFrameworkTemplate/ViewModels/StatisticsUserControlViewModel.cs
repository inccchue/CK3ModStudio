using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Metrics;
using System.Linq;
using LiveCharts;
using LiveCharts.Wpf;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using WpfPrismFrameworkTemplate.Model;

namespace WpfPrismFrameworkTemplate.ViewModels
{
    public enum StatisType
    {
        各文化人数,
        各家族人数
    }
    public class StatisticsUserControlViewModel : BindableBase, INavigationAware
    {
        private ObservableCollection<Family> _FamilyList = new ObservableCollection<Family>();
        private ObservableCollection<County> _Counties = new ObservableCollection<County>();
        private ObservableCollection<People> _TotalPeople = new ObservableCollection<People>();
        public SeriesCollection PieSeriesCollection { get; set; } = new SeriesCollection();
        public ChartValues<int> SeriesValues { get; set; } = new ChartValues<int>();
        public ObservableCollection<string> XLabels { get; set; } = new ObservableCollection<string>() { };
        public DelegateCommand<string> SelectCmd { get; private set; }
        public StatisType _SelectedStatisType = StatisType.各家族人数;
        public StatisticsUserControlViewModel()
        {
            SelectCmd = new DelegateCommand<string>(Select);
        }
        public StatisType SelectedStatisType
        {
            get => _SelectedStatisType;
            set => SetProperty(ref _SelectedStatisType, value);
        }
        private void Select(string statisName)
        {
            try
            {
                if (Enum.TryParse(statisName, out StatisType result))
                {
                    SelectedStatisType = result;
                }
                else
                {
                    Console.WriteLine($"无法将\"{statisName}\"转换为StatisType枚举值。");
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine($"{ex.Message}");
            }
        }
        public void PopulateChartData()
        {
            // 清空现有数据
            XLabels.Clear();
            SeriesValues.Clear();
            PieSeriesCollection.Clear();



            // 填充 XLabels 和 SeriesValues
            foreach (var item in FamilyList)
            {
                // 将 DateTime 转换为字符串并添加到 XLabels
                XLabels.Add(item.FamilyName);

                // 添加 int 值到 SeriesValues
                SeriesValues.Add(item.Members.Count());

                TotalPeople.AddRange(item.Members);
            }

            var groupedData = TotalPeople
                .GroupBy(p => p.Culture) // 根据 Culture 分组
                .Select(group => new { Culture = group.Key, Count = group.Count() }) // 统计数量
                .ToList();

            // 将数据添加到 SeriesCollection
            foreach (var data in groupedData)
            {
                PieSeriesCollection.Add(new PieSeries
                {
                    Title = data.Culture.ToString(), // 分组名称
                    Values = new ChartValues<int> { data.Count }, // 统计的数量
                    DataLabels = true // 显示标签
                });
            }
        }
        public ObservableCollection<Family> FamilyList
        {
            get => _FamilyList;
            set 
            {
                SetProperty(ref _FamilyList, value);
                PopulateChartData();
            } 
        }
        public ObservableCollection<County> Counties
        {
            get => _Counties;
            set => SetProperty(ref _Counties, value);
        }

        public ObservableCollection<People> TotalPeople
        {
            get => _TotalPeople;
            set => SetProperty(ref _TotalPeople, value);
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
