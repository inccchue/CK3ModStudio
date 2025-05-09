using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
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
        public ObservableCollection<TimelineEntry> _TimelineEntries = new ObservableCollection<TimelineEntry>();
        public DelegateCommand<TimelineEntry> DelCmd { get; private set; }
        public CountyTimelineUserControlViewModel()
        {
            DelCmd = new DelegateCommand<TimelineEntry>(Del);
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

        public void Del(TimelineEntry timelineEntry)
        {
            TimelineEntries.Remove(timelineEntry);
            SelectCounty.HolderEntries.Remove(SelectCounty.HolderEntries.FirstOrDefault(e => e.StartDate == timelineEntry.StartDate && e.Holder == timelineEntry.Holder));
            ProcessTimelineEntries();
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
