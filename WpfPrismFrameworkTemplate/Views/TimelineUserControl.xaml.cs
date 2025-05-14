using System.Collections.Specialized;
using System;
using System.Windows.Controls;
using WpfPrismFrameworkTemplate.ViewModels;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Media;

namespace WpfPrismFrameworkTemplate.Views
{
    /// <summary>
    /// Interaction logic for TimelineUserControl
    /// </summary>
    public partial class TimelineUserControl : UserControl
    {
        private TimelineUserControlViewModel _viewModel;
        public TimelineUserControl()
        {
            InitializeComponent();
            Loaded += TimelineControl_Loaded;
        }

        private void TimelineControl_Loaded(object sender, RoutedEventArgs e)
        {
            _viewModel = DataContext as TimelineUserControlViewModel;
            if (_viewModel == null)
                return;

            _viewModel.PropertyChanged += (s, args) =>
            {
                if (args.PropertyName == nameof(TimelineUserControlViewModel.SelectedCounty) ||
                    args.PropertyName == nameof(TimelineUserControlViewModel.MinYear) ||
                    args.PropertyName == nameof(TimelineUserControlViewModel.MaxYear))
                {
                    DrawTimeline();
                    UpdateHolderSegments();
                }
            };

            // Handle when the holder periods collection changes
            if (_viewModel.SelectedCounty != null)
            {
                (_viewModel.SelectedCounty.HolderPeriods as INotifyCollectionChanged).CollectionChanged += (s, args) =>
                {
                    UpdateHolderSegments();
                };
            }

            DrawTimeline();
            UpdateHolderSegments();
        }

        private void DrawTimeline()
        {
            if (_viewModel == null)
                return;

            TimelineAxis.Children.Clear();

            // Add base line
            var baseLine = new Line
            {
                X1 = 0,
                Y1 = 10,
                X2 = _viewModel.TimelineWidth,
                Y2 = 10,
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };
            TimelineAxis.Children.Add(baseLine);

            // Add year ticks
            int tickSpacing = CalculateTickSpacing(_viewModel.MinYear, _viewModel.MaxYear);
            for (int year = _viewModel.MinYear; year <= _viewModel.MaxYear; year += tickSpacing)
            {
                double xPos = _viewModel.CalculatePositionOnTimeline(year);

                var tick = new Line
                {
                    X1 = xPos,
                    Y1 = 5,
                    X2 = xPos,
                    Y2 = 15,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1
                };
                TimelineAxis.Children.Add(tick);

                var yearLabel = new TextBlock
                {
                    Text = year.ToString(),
                    FontSize = 10
                };

                Canvas.SetLeft(yearLabel, xPos - 15);
                Canvas.SetTop(yearLabel, 15);
                TimelineAxis.Children.Add(yearLabel);
            }
        }

        private int CalculateTickSpacing(int minYear, int maxYear)
        {
            int range = maxYear - minYear;

            if (range <= 50) return 5;
            if (range <= 100) return 10;
            if (range <= 200) return 20;
            if (range <= 500) return 50;

            return 100;
        }

        private void UpdateHolderSegments()
        {
            if (_viewModel?.SelectedCounty == null)
                return;
            // The positioning of segments is done through a custom panel layout or through binding transformations
            for (int i = 0; i < _viewModel.SelectedCounty.HolderPeriods.Count; i++)
            {
                var currentPeriod = _viewModel.SelectedCounty.HolderPeriods[i];

                // 修改这一行，用传统的 if-else 替代条件运算符
                int? nextPeriodYear = null;
                if (i < _viewModel.SelectedCounty.HolderPeriods.Count - 1)
                {
                    nextPeriodYear = _viewModel.SelectedCounty.HolderPeriods[i + 1].Year;
                }

                var container = HolderSegments.ItemContainerGenerator.ContainerFromIndex(i) as ContentPresenter;
                if (container != null)
                {
                    double xPos = _viewModel.CalculatePositionOnTimeline(currentPeriod.Year);
                    double width = _viewModel.CalculateWidthOnTimeline(currentPeriod.Year, nextPeriodYear);
                    Canvas.SetLeft(container, xPos);
                    container.Width = Math.Max(50, width); // Ensure minimum width for visibility
                }
            }
        }
    }
}
