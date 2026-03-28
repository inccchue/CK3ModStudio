using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using LiveCharts;
using LiveCharts.Wpf;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using WpfPrismFrameworkTemplate.Helper;
using WpfPrismFrameworkTemplate.Model;

namespace WpfPrismFrameworkTemplate.ViewModels
{
    public enum StatisType
    {
        各文化人数,
        各家族人数,
        各宗教人数,
        性别比例,
        各家族平均寿命
    }
    public class StatisticsUserControlViewModel : BindableBase, INavigationAware
    {
        private ObservableCollection<Family> _FamilyList = new ObservableCollection<Family>();
        private ObservableCollection<County> _Counties = new ObservableCollection<County>();
        public StatisType _SelectedStatisType = StatisType.各家族人数;

        // 各家族人数
        public ChartValues<int> SeriesValues { get; set; } = new ChartValues<int>();
        public ObservableCollection<string> XLabels { get; set; } = new ObservableCollection<string>();

        // 各文化人数
        public SeriesCollection PieSeriesCollection { get; set; } = new SeriesCollection();

        // 各宗教人数
        public SeriesCollection ReligionPieSeriesCollection { get; set; } = new SeriesCollection();

        // 性别比例
        public SeriesCollection GenderPieSeriesCollection { get; set; } = new SeriesCollection();

        // 各家族平均寿命
        public ChartValues<double> AvgAgeValues { get; set; } = new ChartValues<double>();
        public ObservableCollection<string> AvgAgeLabels { get; set; } = new ObservableCollection<string>();

        public DelegateCommand<string> SelectCmd { get; private set; }

        public StatisticsUserControlViewModel()
        {
            SelectCmd = new DelegateCommand<string>(Select);
        }

        public StatisType SelectedStatisType
        {
            get => _SelectedStatisType;
            set => SetProperty(ref _SelectedStatisType, value);
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

        private void Select(string statisName)
        {
            if (Enum.TryParse(statisName, out StatisType result))
            {
                SelectedStatisType = result;
            }
        }

        public void PopulateChartData()
        {
            XLabels.Clear();
            SeriesValues.Clear();
            PieSeriesCollection.Clear();
            ReligionPieSeriesCollection.Clear();
            GenderPieSeriesCollection.Clear();
            AvgAgeValues.Clear();
            AvgAgeLabels.Clear();

            var allPeople = FamilyList.SelectMany(f => f.Members).ToList();

            // 各家族人数（柱状图）
            foreach (var family in FamilyList)
            {
                XLabels.Add(family.FamilyName);
                SeriesValues.Add(family.Members.Count);
            }

            // 各文化人数（饼图）
            var cultureGroups = allPeople
                .GroupBy(p => string.IsNullOrEmpty(p.Culture) ? "未知" : p.Culture)
                .Select(g => new { Name = g.Key, Count = g.Count() });
            foreach (var item in cultureGroups)
            {
                PieSeriesCollection.Add(new PieSeries
                {
                    Title = item.Name,
                    Values = new ChartValues<int> { item.Count },
                    DataLabels = true
                });
            }

            // 各宗教人数（饼图）
            var religionGroups = allPeople
                .GroupBy(p => string.IsNullOrEmpty(p.Religion) ? "未知" : p.Religion)
                .Select(g => new { Name = g.Key, Count = g.Count() });
            foreach (var item in religionGroups)
            {
                ReligionPieSeriesCollection.Add(new PieSeries
                {
                    Title = item.Name,
                    Values = new ChartValues<int> { item.Count },
                    DataLabels = true
                });
            }

            // 性别比例（饼图）
            int maleCount = allPeople.Count(p => p.Gender == GenderType.Male);
            int femaleCount = allPeople.Count(p => p.Gender == GenderType.Female);
            if (maleCount > 0)
                GenderPieSeriesCollection.Add(new PieSeries { Title = "男", Values = new ChartValues<int> { maleCount }, DataLabels = true });
            if (femaleCount > 0)
                GenderPieSeriesCollection.Add(new PieSeries { Title = "女", Values = new ChartValues<int> { femaleCount }, DataLabels = true });

            // 各家族平均寿命（柱状图，仅统计有出生和死亡记录的人）
            foreach (var family in FamilyList)
            {
                var ages = family.Members
                    .Select(p =>
                    {
                        var birth = p.LifeEventList.FirstOrDefault(e => e.EventType == LifeEventType.Birth);
                        var death = p.LifeEventList.FirstOrDefault(e => e.EventType == LifeEventType.Death);
                        if (birth == null || death == null) return -1.0;
                        DateTime birthDate = CommonHelper.ParseDate(birth.EventDate);
                        DateTime deathDate = CommonHelper.ParseDate(death.EventDate);
                        if (birthDate == DateTime.MinValue || deathDate == DateTime.MinValue || deathDate <= birthDate)
                            return -1.0;
                        return (deathDate - birthDate).TotalDays / 365.25;
                    })
                    .Where(age => age > 0)
                    .ToList();

                if (ages.Count > 0)
                {
                    AvgAgeLabels.Add(family.FamilyName);
                    AvgAgeValues.Add(Math.Round(ages.Average(), 1));
                }
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

        public bool IsNavigationTarget(NavigationContext navigationContext) => true;

        public void OnNavigatedFrom(NavigationContext navigationContext) { }
    }
}
