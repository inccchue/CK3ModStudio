using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using WpfPrismFrameworkTemplate.Helper;
using WpfPrismFrameworkTemplate.Model;
using WpfPrismFrameworkTemplate.ViewModels;

namespace WpfPrismFrameworkTemplate.Views
{
    public partial class GenerateSubmodDialog : Window
    {
        private readonly GenerateSubmodViewModel _vm;
        private readonly LandedTitle _county;

        public GenerateSubmodDialog(LandedTitle county)
        {
            _county = county;
            _vm = new GenerateSubmodViewModel();
            // 先填充参考伯国/文化/宗教默认值，再绑定 DataContext
            _vm.SetDefaultsFromCounty(county);
            DataContext = _vm;
            InitializeComponent();
            PopulateFromCounty();
            Loaded += async (s, e) => await _vm.LoadCulturesAndReligionsAsync();
        }

        private void PopulateFromCounty()
        {
            _vm.CountyKey = _county.Key;
            _vm.DuchyKey = _county.Parent != null ? _county.Parent.Key : "";
            _vm.KingdomKey = (_county.Parent != null && _county.Parent.Parent != null)
                ? _county.Parent.Parent.Key : "";

            TxtCountyKey.Text = _vm.CountyKey;
            TxtDuchyKey.Text = _vm.DuchyKey;
            TxtKingdomKey.Text = _vm.KingdomKey;
            TxtCountyKeyHeader.Text = _vm.CountyKey + " (" + _county.RankLabel + ")";

            foreach (var child in _county.Children)
            {
                if (child.Rank == TitleRank.Barony)
                {
                    _vm.Baronies.Add(new BaronyInput
                    {
                        Key = child.Key,
                        ProvinceId = child.Province
                    });
                }
            }
        }

        // ── 文件夹选择 ───────────────────────────────────────────────────

        private void BrowseSubmod_Click(object sender, RoutedEventArgs e)
        {
            string path = BrowseFolder("选择子mod文件夹", _vm.SubmodPath);
            if (path != null) _vm.SubmodPath = path;
        }

        private void BrowseAgot_Click(object sender, RoutedEventArgs e)
        {
            string path = BrowseFolder("选择AGOT主mod文件夹", _vm.AgotModPath);
            if (path != null)
            {
                _vm.AgotModPath = path;
                // AgotModPath setter 会自动触发 LoadCulturesAndReligionsAsync
            }
        }

        private static string BrowseFolder(string description, string initialPath)
        {
            var dlg = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = description,
                ShowNewFolderButton = false,
                SelectedPath = System.IO.Directory.Exists(initialPath) ? initialPath : ""
            };
            return dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK
                ? dlg.SelectedPath : null;
        }

        // ── 生成 ─────────────────────────────────────────────────────────

        private void Generate_Click(object sender, RoutedEventArgs e)
        {
            TxtStatus.Text = "正在生成...";
            TxtStatus.Foreground = System.Windows.Media.Brushes.LightBlue;

            var generator = new SubmodFileGenerator(_vm.SubmodPath, _vm.AgotModPath);
            var baronies = new List<BaronyInput>(_vm.Baronies);

            var result = generator.Generate(
                _county,
                _vm.Culture,
                _vm.Religion,
                _vm.DevelopmentLevel,
                _vm.EnglishName,
                _vm.ChineseName,
                _vm.ReferenceCountyKey,
                _vm.TitleHistoryRegion,
                baronies);

            if (result.Success)
            {
                TxtStatus.Text = result.Message + "\n" + string.Join("\n", result.GeneratedFiles);
                TxtStatus.Foreground = System.Windows.Media.Brushes.LightGreen;
            }
            else
            {
                TxtStatus.Text = result.Message;
                TxtStatus.Foreground = System.Windows.Media.Brushes.Salmon;
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
