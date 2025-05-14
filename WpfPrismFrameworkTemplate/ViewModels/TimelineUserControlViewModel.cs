using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Web.UI.WebControls;
using Microsoft.Win32;
using System.Windows.Input;
using Prism.Commands;
using Prism.Mvvm;
using WpfPrismFrameworkTemplate.Model;

namespace WpfPrismFrameworkTemplate.ViewModels
{
    public class TimelineUserControlViewModel : BindableBase
    {
        private readonly CountyDataParser _dataParser;
        private ObservableCollection<County> _counties;
        private County _selectedCounty;
        private int _minYear = 7800;
        private int _maxYear = 8300;
        private int _timelineWidth = 1000;
        public TimelineUserControlViewModel(CountyDataParser dataParser)
        {
            _dataParser = dataParser;
            Counties = new ObservableCollection<County>();
            LoadFileCommand = new DelegateCommand(OnLoadFile);
            LoadSampleDataCommand = new DelegateCommand(OnLoadSampleData);
        }

        public ObservableCollection<County> Counties
        {
            get => _counties;
            set => SetProperty(ref _counties, value);
        }

        public County SelectedCounty
        {
            get => _selectedCounty;
            set => SetProperty(ref _selectedCounty, value);
        }

        public int MinYear
        {
            get => _minYear;
            set => SetProperty(ref _minYear, value);
        }

        public int MaxYear
        {
            get => _maxYear;
            set => SetProperty(ref _maxYear, value);
        }

        public int TimelineWidth
        {
            get => _timelineWidth;
            set => SetProperty(ref _timelineWidth, value);
        }

        public ICommand LoadFileCommand { get; }
        public ICommand LoadSampleDataCommand { get; }

        private void OnLoadFile()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                Title = "Select County Data File"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    var loadedCounties = _dataParser.ParseFromFile(openFileDialog.FileName);
                    Counties = new ObservableCollection<County>(loadedCounties);

                    if (Counties.Any())
                    {
                        SelectedCounty = Counties.First();
                        UpdateTimelineRange();
                    }
                }
                catch (Exception ex)
                {
                    // In a real application, show error message to user
                    System.Diagnostics.Debug.WriteLine($"Error loading file: {ex.Message}");
                }
            }
        }

        private void OnLoadSampleData()
        {
            // Sample data from the example
            string sampleData = @"c_wansford = { 	 	8060.1.1 = { 		holder = Inchfield_25 	} 	8105.1.1 = { 		holder = Inchfield_27 	} 	8137.1.1 = { 		holder = Inchfield_35 	} 	8145.1.1 = { 		holder = Inchfield_46 	} 	8172.1.1 = { 		holder = Inchfield_48 	} 	8174.1.1 = { 		holder = Inchfield_47 	} 	8205.1.1 = { 		holder = Inchfield_52 	} 	8236.1.1 = { 		holder = Inchfield_53 	} 	8243.1.1 = { 		holder = Inchfield_56 	} 	8291.1.1 = { 		holder = Inchfield_1 	} } c_uplands = {  	7870.1.1 = { 		holder = Mullendore_6 	} 	7921.1.1 = { 		holder = Mullendore_8 	} 	7947.1.1 = { 		holder = Mullendore_9 	} 	7974.1.1 = { 		holder = Mullendore_12 	} 	7980.1.1 = { 		holder = Mullendore_22 	} 	8025.1.1 = { 		holder = Mullendore_27 	} 	8049.1.1 = { 		holder = Mullendore_30 	} 	8063.1.1 = { 		holder = Mullendore_33 	} 	8101.1.1 = { 		holder = Mullendore_36 	} 	8108.1.1 = { 		holder = Mullendore_38 	} 	8148.1.1 = { 		holder = Mullendore_40 	} 	8169.1.1 = { 		holder = Mullendore_45 	} 	8206.1.1 = { 		holder = Hatherleigh_1 	} 	8258.1.1 = { 		holder = Hatherleigh_2 	} } c_haxby = {  	8001.1.1 = { 		holder = Inchfield_16 	} 	8029.1.1 = { 		holder = Inchfield_18 	} 	8058.1.1 = { 		holder = Inchfield_23 	} 	8060.1.1 = { 		holder = Inchfield_25 	} 	8105.1.1 = { 		holder = Inchfield_27 	} 	8137.1.1 = { 		holder = Inchfield_35 	} 	8145.1.1 = { 		holder = Inchfield_46 	} 	8172.1.1 = { 		holder = Inchfield_48 	} 	8174.1.1 = { 		holder = Inchfield_47 	} 	8205.1.1 = { 		holder = Inchfield_52 	} 	8236.1.1 = { 		holder = Inchfield_53 	} 	8243.1.1 = { 		holder = Inchfield_56 	} 	8291.1.1 = { 		holder = Inchfield_1 	} }";

            var loadedCounties = _dataParser.Parse(sampleData);
            Counties = new ObservableCollection<County>(loadedCounties);

            if (Counties.Any())
            {
                SelectedCounty = Counties.First();
                UpdateTimelineRange();
            }
        }

        private void UpdateTimelineRange()
        {
            if (Counties.Count == 0)
                return;

            var allPeriods = Counties.SelectMany(c => c.HolderPeriods).ToList();
            if (allPeriods.Any())
            {
                MinYear = allPeriods.Min(p => p.Year);
                MaxYear = allPeriods.Max(p => p.Year);
            }
        }

        public double CalculatePositionOnTimeline(int year)
        {
            if (MaxYear <= MinYear)
                return 0;

            return ((double)(year - MinYear) / (MaxYear - MinYear)) * TimelineWidth;
        }

        public double CalculateWidthOnTimeline(int startYear, int? endYear)
        {
            if (MaxYear <= MinYear)
                return 0;

            int end = endYear ?? MaxYear;
            return ((double)(end - startYear) / (MaxYear - MinYear)) * TimelineWidth;
        }
    }
}
