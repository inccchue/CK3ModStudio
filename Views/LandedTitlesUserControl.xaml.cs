using Microsoft.Win32;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WpfPrismFrameworkTemplate.Model;
using WpfPrismFrameworkTemplate.ViewModels;

namespace WpfPrismFrameworkTemplate.Views
{
    public partial class LandedTitlesUserControl : UserControl
    {
        private LandedTitlesViewModel VM => DataContext as LandedTitlesViewModel;

        private Point _dragStartPoint;
        private bool _isDragging;
        private Border _dragOverBorder;
        private bool _updatingCapital;

        public LandedTitlesUserControl()
        {
            InitializeComponent();
            DataContext = new LandedTitlesViewModel();
            Loaded += OnLoaded;
        }

        private async void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            Loaded -= OnLoaded;
            var lastFile = VM?.GetLastFilePath();
            if (!string.IsNullOrEmpty(lastFile) && System.IO.File.Exists(lastFile))
            {
                IsEnabled = false;
                try { await VM.LoadFileAsync(lastFile); }
                catch { /* 忽略自动加载失败，不影响正常使用 */ }
                finally { IsEnabled = true; }
            }
        }

        // ── 树选择 ─────────────────────────────────────────────────────

        private void TitleTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var title = e.NewValue as LandedTitle;
            if (VM != null) VM.SelectedTitle = title;
            BtnMove.IsEnabled = title != null;
            UpdateDetailPanel(title);
        }

        private void UpdateDetailPanel(LandedTitle title)
        {
            if (title == null)
            {
                NoSelectionText.Visibility = Visibility.Visible;
                DetailContent.Visibility = Visibility.Collapsed;
                return;
            }

            NoSelectionText.Visibility = Visibility.Collapsed;
            DetailContent.Visibility = Visibility.Visible;

            // 等级标头
            TxtRankIcon.Text = title.RankIcon;
            TxtRankLabel.Text = title.RankLabel;
            TxtTitleKey.Text = title.Key;
            RankHeader.Background = new SolidColorBrush(Darken(title.RankColor, 0.6));

            // 属性
            TxtParent.Text = title.Parent?.Key ?? "(根级)";
            TxtProvinceDetail.Text = title.Province.HasValue ? title.Province.ToString() : "-";
            TxtChildCount.Text = title.Children.Count.ToString();
            ColorSwatch.Background = new SolidColorBrush(title.Color);
            TxtColorRgb.Text = string.Format("RGB({0},{1},{2})", title.Color.R, title.Color.G, title.Color.B);

            // 生成子mod按钮只对伯国显示
            BtnGenerateSubmod.Visibility = title.Rank == TitleRank.County
                ? Visibility.Visible : Visibility.Collapsed;

            // 首都显示：伯国取第一个男爵领，其他取 Capital 字段
            if (title.Rank == TitleRank.County)
            {
                var firstBarony = title.Children.FirstOrDefault(c => c.Rank == TitleRank.Barony);
                TxtCapitalDetail.Text = firstBarony != null ? firstBarony.Key : "-";

                // 显示调整首都的下拉框
                var baronies = title.Children.Where(c => c.Rank == TitleRank.Barony).ToList();
                CapitalPanel.Visibility = baronies.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
                _updatingCapital = true;
                CapitalCombo.ItemsSource = baronies;
                CapitalCombo.SelectedIndex = baronies.Count > 0 ? 0 : -1;
                _updatingCapital = false;
            }
            else
            {
                TxtCapitalDetail.Text = string.IsNullOrEmpty(title.Capital) ? "-" : title.Capital;
                CapitalPanel.Visibility = Visibility.Collapsed;
                CapitalCombo.ItemsSource = null;
            }
        }

        private void CapitalCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_updatingCapital) return;
            var title = VM?.SelectedTitle;
            if (title == null || title.Rank != TitleRank.County) return;
            var barony = CapitalCombo.SelectedItem as LandedTitle;
            if (barony == null || title.Children.IndexOf(barony) == 0) return;

            // 将选中的男爵领移至子节点列表首位，使其成为序列化时的首都
            title.Children.Remove(barony);
            title.Children.Insert(0, barony);

            // 更新首都显示
            TxtCapitalDetail.Text = barony.Key;
            if (VM != null) VM.IsDirty = true;
        }

        // 将颜色变暗（用于标头背景，使图标更清晰）
        private static Color Darken(Color c, double factor)
        {
            return Color.FromRgb(
                (byte)(c.R * factor),
                (byte)(c.G * factor),
                (byte)(c.B * factor));
        }

        // ── 拖拽 ───────────────────────────────────────────────────────

        private void TitleItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _dragStartPoint = e.GetPosition(null);
            _isDragging = false;
        }

        private void TitleItem_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed || _isDragging) return;

            var pos = e.GetPosition(null);
            if (Math.Abs(pos.X - _dragStartPoint.X) < SystemParameters.MinimumHorizontalDragDistance &&
                Math.Abs(pos.Y - _dragStartPoint.Y) < SystemParameters.MinimumVerticalDragDistance)
                return;

            var border = sender as Border;
            var title = border?.Tag as LandedTitle;
            if (title == null || VM == null || title.Parent == null) return;

            _isDragging = true;
            VM.DragSource = title;
            DragDrop.DoDragDrop(border, title, DragDropEffects.Move);
            _isDragging = false;
            VM.DragSource = null;
        }

        private void TitleItem_DragEnter(object sender, DragEventArgs e)
        {
            var border = sender as Border;
            if (border == null || VM == null) return;

            var target = border.Tag as LandedTitle;
            if (IsValidDrop(VM.DragSource, target))
            {
                _dragOverBorder = border;
                border.BorderBrush = new SolidColorBrush(Color.FromRgb(126, 179, 247));
                border.BorderThickness = new Thickness(1);
                e.Effects = DragDropEffects.Move;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void TitleItem_DragLeave(object sender, DragEventArgs e)
        {
            ClearDropHighlight(sender as Border);
        }

        private void TitleItem_Drop(object sender, DragEventArgs e)
        {
            ClearDropHighlight(sender as Border);
            if (VM == null) return;

            var target = (sender as Border)?.Tag as LandedTitle;
            var source = VM.DragSource;
            if (!IsValidDrop(source, target)) return;
            if (VM.MoveTitle(source, target))
                UpdateDetailPanel(source);
        }

        private void ClearDropHighlight(Border border)
        {
            if (border == null) return;
            border.BorderBrush = Brushes.Transparent;
            border.BorderThickness = new Thickness(0);
            if (_dragOverBorder == border) _dragOverBorder = null;
        }

        private bool IsValidDrop(LandedTitle source, LandedTitle target)
        {
            if (source == null || target == null || source == target) return false;
            if ((int)source.Rank <= (int)target.Rank) return false;
            return true;
        }

        // ── 工具栏 ─────────────────────────────────────────────────────

        private async void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Title = "打开法理文件",
                Filter = "CK3法理文件 (*.txt)|*.txt|所有文件 (*.*)|*.*"
            };
            if (dlg.ShowDialog() != true) return;
            IsEnabled = false;
            try
            {
                await VM.LoadFileAsync(dlg.FileName);
            }
            catch (Exception ex)
            {
                HandyControl.Controls.Growl.Error("加载失败: " + ex.Message);
            }
            finally
            {
                IsEnabled = true;
            }
        }

        private void SaveFile_Click(object sender, RoutedEventArgs e)
        {
            if (VM == null) return;
            if (string.IsNullOrEmpty(VM.CurrentFilePath))
                SaveAsFile_Click(sender, e);
            else
                VM.SaveFile(VM.CurrentFilePath);
        }

        private void SaveAsFile_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog
            {
                Title = "保存法理文件",
                Filter = "CK3法理文件 (*.txt)|*.txt|所有文件 (*.*)|*.*",
                FileName = "landed_titles.txt"
            };
            if (dlg.ShowDialog() == true)
                VM?.SaveFile(dlg.FileName);
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (VM != null) VM.SearchText = TxtSearch.Text;
            SearchHint.Visibility = string.IsNullOrEmpty(TxtSearch.Text)
                ? Visibility.Visible : Visibility.Collapsed;
        }

        private void MoveTitle_Click(object sender, RoutedEventArgs e)
        {
            if (VM?.SelectedTitle == null) return;
            var source = VM.SelectedTitle;

            if (source.Parent == null)
            {
                HandyControl.Controls.Growl.Warning("根级帝国法理无法移动");
                return;
            }

            var parentRank = (TitleRank)((int)source.Rank - 1);
            var targets = VM.GetTitlesOfRank(parentRank);
            if (targets.Count == 0)
            {
                HandyControl.Controls.Growl.Warning("没有合适的目标父级");
                return;
            }

            var vm = new MoveLandedTitleViewModel(source, targets);
            var dlg = new MoveLandedTitleDialog(vm) { Owner = Window.GetWindow(this) };
            if (dlg.ShowDialog() == true && vm.SelectedTarget != null)
                VM.MoveTitle(source, vm.SelectedTarget);
        }

        private void AddEmpire_Click(object sender, RoutedEventArgs e) => OpenAddDialog(TitleRank.Empire, null);
        private void AddKingdom_Click(object sender, RoutedEventArgs e) => OpenAddDialog(TitleRank.Kingdom, null);
        private void AddDuchy_Click(object sender, RoutedEventArgs e) => OpenAddDialog(TitleRank.Duchy, null);
        private void AddCounty_Click(object sender, RoutedEventArgs e) => OpenAddDialog(TitleRank.County, null);

        private void AddChildTitle_Click(object sender, RoutedEventArgs e)
        {
            var parent = VM?.SelectedTitle;
            if (parent == null) return;
            var childRank = (TitleRank)((int)parent.Rank + 1);
            if (childRank > TitleRank.Barony) return;
            OpenAddDialog(childRank, parent);
        }

        private void OpenAddDialog(TitleRank rank, LandedTitle suggestedParent)
        {
            if (VM == null) return;
            var dlg = new AddLandedTitleDialog(rank, VM.GetAllTitles(), suggestedParent)
            {
                Owner = Window.GetWindow(this)
            };
            if (dlg.ShowDialog() == true && dlg.ResultTitle != null)
                VM.AddTitle(dlg.ResultTitle, dlg.ResultParent);
        }

        private void CopyKey_Click(object sender, RoutedEventArgs e)
        {
            var key = VM?.SelectedTitle?.Key;
            if (!string.IsNullOrEmpty(key))
                Clipboard.SetText(key);
        }

        private void GenerateSubmod_Click(object sender, RoutedEventArgs e)
        {
            var county = VM?.SelectedTitle;
            if (county == null || county.Rank != TitleRank.County) return;

            var dlg = new GenerateSubmodDialog(county) { Owner = Window.GetWindow(this) };
            dlg.ShowDialog();
        }
    }
}
