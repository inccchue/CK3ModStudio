using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WpfPrismFrameworkTemplate.Model;

namespace WpfPrismFrameworkTemplate.Views
{
    public partial class AddLandedTitleDialog : Window
    {
        public LandedTitle ResultTitle { get; private set; }
        public LandedTitle ResultParent { get; private set; }

        private TitleRank _rank = TitleRank.County;
        private readonly HashSet<string> _existingKeys;
        private List<LandedTitle> _allParentCandidates = new List<LandedTitle>();

        private Border[] _rankBtns;
        private readonly string[] _rankTags = { "Empire", "Kingdom", "Duchy", "County" };
        private readonly TitleRank[] _rankValues =
        {
            TitleRank.Empire, TitleRank.Kingdom, TitleRank.Duchy, TitleRank.County
        };

        public AddLandedTitleDialog(TitleRank initialRank,
                                    IEnumerable<LandedTitle> allTitles,
                                    LandedTitle suggestedParent = null)
        {
            InitializeComponent();

            _rankBtns = new[] { BtnEmpire, BtnKingdom, BtnDuchy, BtnCounty };

            _existingKeys = new HashSet<string>(
                Flatten(allTitles).Select(t => t.Key),
                StringComparer.OrdinalIgnoreCase);

            _allParentCandidates = Flatten(allTitles).ToList();

            SelectRank(initialRank);

            if (suggestedParent != null)
            {
                foreach (var item in ParentList.Items)
                {
                    if (item is LandedTitle t && t == suggestedParent)
                    {
                        ParentList.SelectedItem = t;
                        break;
                    }
                }
            }

            TxtKeySuffix.Focus();
        }

        private void RankBtn_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is Border b)
            {
                var tag = b.Tag?.ToString() ?? "";
                for (int i = 0; i < _rankTags.Length; i++)
                    if (_rankTags[i] == tag) { SelectRank(_rankValues[i]); return; }
            }
        }

        private void SelectRank(TitleRank rank)
        {
            _rank = rank;

            for (int i = 0; i < _rankBtns.Length; i++)
            {
                bool selected = _rankValues[i] == rank;
                _rankBtns[i].Background = selected
                    ? new SolidColorBrush(Color.FromRgb(55, 69, 90))
                    : new SolidColorBrush(Color.FromRgb(49, 50, 68));
                _rankBtns[i].BorderThickness = selected ? new Thickness(2) : new Thickness(0);
                _rankBtns[i].BorderBrush = selected
                    ? new SolidColorBrush(Color.FromRgb(137, 180, 250))
                    : Brushes.Transparent;
            }

            string prefix;
            switch (rank)
            {
                case TitleRank.Empire: prefix = "e_"; break;
                case TitleRank.Kingdom: prefix = "k_"; break;
                case TitleRank.Duchy: prefix = "d_"; break;
                case TitleRank.County: prefix = "c_"; break;
                default: prefix = "b_"; break;
            }
            TxtPrefix.Text = prefix;
            PrefixBox.Text = prefix;

            ParentPanel.Visibility = (rank == TitleRank.Empire)
                ? Visibility.Collapsed
                : Visibility.Visible;

            RefreshParentList(TxtParentSearch.Text);
            UpdatePreview();
            ValidateAndUpdateConfirm();
        }

        private void KeySuffix_Changed(object sender, TextChangedEventArgs e)
        {
            UpdatePreview();
            ValidateAndUpdateConfirm();
        }

        private void UpdatePreview()
        {
            var suffix = TxtKeySuffix.Text.Trim();
            TxtPreview.Text = string.IsNullOrEmpty(suffix)
                ? "…"
                : suffix;
        }

        private void Color_Changed(object sender, TextChangedEventArgs e)
        {
            if (TxtR == null || TxtG == null || TxtB == null || ColorPreview == null) return;
            byte r, g, b;
            if (byte.TryParse(TxtR.Text, out r) &&
                byte.TryParse(TxtG.Text, out g) &&
                byte.TryParse(TxtB.Text, out b))
            {
                ColorPreview.Background = new SolidColorBrush(Color.FromRgb(r, g, b));
            }
        }

        private void ParentSearch_Changed(object sender, TextChangedEventArgs e)
        {
            ParentSearchHint.Visibility = string.IsNullOrEmpty(TxtParentSearch.Text)
                ? Visibility.Visible : Visibility.Collapsed;
            RefreshParentList(TxtParentSearch.Text);
        }

        private void RefreshParentList(string filter)
        {
            var parentRank = (TitleRank)((int)_rank - 1);
            if ((int)_rank == 0) return;

            var items = _allParentCandidates
                .Where(t => t.Rank == parentRank)
                .Where(t => string.IsNullOrWhiteSpace(filter) ||
                            t.Key.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)
                .OrderBy(t => t.Key)
                .ToList();

            ParentList.ItemsSource = items;
            if (items.Count > 0) ParentList.SelectedIndex = 0;
            ValidateAndUpdateConfirm();
        }

        private void ParentList_SelectionChanged(object sender, SelectionChangedEventArgs e)
            => ValidateAndUpdateConfirm();

        private void ValidateAndUpdateConfirm()
        {
            if (TxtPreview == null || BtnConfirm == null) return;

            var suffix = TxtKeySuffix?.Text.Trim() ?? "";
            var fullKey = string.Format("{0}{1}", TxtPrefix?.Text ?? "", suffix);

            string error = null;
            if (string.IsNullOrWhiteSpace(suffix))
                error = "键值后缀不能为空";
            else if (suffix.Any(char.IsWhiteSpace))
                error = "键值不能含空格";
            else if (_existingKeys.Contains(fullKey))
                error = string.Format("键值 {0} 已存在", fullKey);
            else if (_rank != TitleRank.Empire && ParentList.SelectedItem == null)
                error = "请选择父级";

            if (error != null)
            {
                ErrBorder.Visibility = Visibility.Visible;
                TxtError.Text = error;
                BtnConfirm.IsEnabled = false;
            }
            else
            {
                ErrBorder.Visibility = Visibility.Collapsed;
                BtnConfirm.IsEnabled = true;
            }
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            var suffix = TxtKeySuffix.Text.Trim();
            var fullKey = string.Format("{0}{1}", TxtPrefix.Text, suffix);

            byte r = 128, g = 128, b = 128;
            byte.TryParse(TxtR.Text, out r);
            byte.TryParse(TxtG.Text, out g);
            byte.TryParse(TxtB.Text, out b);

            ResultTitle = new LandedTitle
            {
                Key = fullKey,
                Rank = _rank,
                Capital = TxtCapital.Text.Trim(),
                Color = Color.FromRgb(r, g, b),
                DefiniteForm = ChkDefinite.IsChecked == true,
            };

            ResultParent = _rank == TitleRank.Empire
                ? null
                : ParentList.SelectedItem as LandedTitle;

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private static IEnumerable<LandedTitle> Flatten(IEnumerable<LandedTitle> titles)
        {
            foreach (var t in titles)
            {
                yield return t;
                foreach (var c in Flatten(t.Children))
                    yield return c;
            }
        }
    }
}
