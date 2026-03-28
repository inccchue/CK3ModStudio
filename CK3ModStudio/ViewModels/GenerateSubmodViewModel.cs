using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Prism.Mvvm;
using WpfPrismFrameworkTemplate.Helper;
using WpfPrismFrameworkTemplate.Model;

namespace WpfPrismFrameworkTemplate.ViewModels
{
    public class GenerateSubmodViewModel : BindableBase
    {
        // 默认路径（首次使用时的回退值）
        private const string DefaultSubmodPath =
            @"C:\Users\19322\Documents\Paradox Interactive\Crusader Kings III\mod\AGOT More Counts[Playfun]";
        private const string DefaultAgotModPath =
            @"D:\Program Files (x86)\Steam\steamapps\workshop\content\1158310\2962333032";

        private string _submodPath;
        private string _agotModPath;
        private string _culture = "";
        private string _religion = "";
        private int _developmentLevel = 10;
        private string _englishName = "";
        private string _chineseName = "";
        private string _referenceCountyKey = "";
        private string _titleHistoryRegion = "";
        private string _countyKey = "";
        private string _duchyKey = "";
        private string _kingdomKey = "";
        private bool _isLoadingLists;

        // 首都男爵领省份ID，用于加载完成后自动填充文化/宗教
        private int? _capitalProvinceId;

        public GenerateSubmodViewModel()
        {
            _submodPath = AppSettings.SubmodPath ?? DefaultSubmodPath;
            _agotModPath = AppSettings.AgotModPath ?? DefaultAgotModPath;
        }

        public string SubmodPath
        {
            get => _submodPath;
            set
            {
                if (SetProperty(ref _submodPath, value))
                    AppSettings.SubmodPath = value;
            }
        }

        public string AgotModPath
        {
            get => _agotModPath;
            set
            {
                if (SetProperty(ref _agotModPath, value))
                {
                    AppSettings.AgotModPath = value;
                    _ = LoadCulturesAndReligionsAsync();
                }
            }
        }

        public string Culture
        {
            get => _culture;
            set => SetProperty(ref _culture, value);
        }

        public string Religion
        {
            get => _religion;
            set => SetProperty(ref _religion, value);
        }

        public int DevelopmentLevel
        {
            get => _developmentLevel;
            set => SetProperty(ref _developmentLevel, value);
        }

        public string EnglishName
        {
            get => _englishName;
            set => SetProperty(ref _englishName, value);
        }

        public string ChineseName
        {
            get => _chineseName;
            set => SetProperty(ref _chineseName, value);
        }

        public string ReferenceCountyKey
        {
            get => _referenceCountyKey;
            set => SetProperty(ref _referenceCountyKey, value);
        }

        public string TitleHistoryRegion
        {
            get => _titleHistoryRegion;
            set => SetProperty(ref _titleHistoryRegion, value);
        }

        public string CountyKey
        {
            get => _countyKey;
            set => SetProperty(ref _countyKey, value);
        }

        public string DuchyKey
        {
            get => _duchyKey;
            set => SetProperty(ref _duchyKey, value);
        }

        public string KingdomKey
        {
            get => _kingdomKey;
            set
            {
                if (SetProperty(ref _kingdomKey, value))
                {
                    if (string.IsNullOrEmpty(TitleHistoryRegion) && !string.IsNullOrEmpty(value))
                        TitleHistoryRegion = value.StartsWith("k_") ? value.Substring(2) : value;
                }
            }
        }

        public bool IsLoadingLists
        {
            get => _isLoadingLists;
            set => SetProperty(ref _isLoadingLists, value);
        }

        public ObservableCollection<BaronyInput> Baronies { get; } = new ObservableCollection<BaronyInput>();
        public ObservableCollection<string> Cultures { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> Religions { get; } = new ObservableCollection<string>();

        /// <summary>
        /// 根据伯国的公国上下文，填充参考伯国键（公国首都）及其文化/宗教默认值。
        /// 必须在 InitializeComponent 之前调用（无需等待文化列表加载完成）。
        /// </summary>
        public void SetDefaultsFromCounty(LandedTitle county)
        {
            var duchy = county.Parent;
            if (duchy == null) return;

            // 首都伯国 = 公国下第一个不是当前伯国的伯国
            var capitalCounty = duchy.Children
                .FirstOrDefault(c => c.Rank == TitleRank.County && c != county);

            if (capitalCounty != null)
            {
                ReferenceCountyKey = capitalCounty.Key;

                // 首都的首都男爵领省份ID（用于加载完成后查文化/宗教）
                var capitalBarony = capitalCounty.Children
                    .FirstOrDefault(b => b.Rank == TitleRank.Barony && b.Province.HasValue);
                _capitalProvinceId = capitalBarony?.Province;
            }
        }

        /// <summary>
        /// 异步加载文化/宗教下拉列表，并在完成后自动填充文化/宗教默认值。
        /// </summary>
        public async Task LoadCulturesAndReligionsAsync()
        {
            IsLoadingLists = true;
            string path = _agotModPath;
            int? provId = _capitalProvinceId;

            var result = await Task.Run(() =>
            {
                List<string> c, r;
                SubmodFileGenerator.ExtractCulturesAndReligions(path, out c, out r);

                // 同时查首都省份的文化/宗教
                string defCulture = null, defReligion = null;
                if (provId.HasValue)
                    SubmodFileGenerator.GetProvinceInfo(path, provId.Value, out defCulture, out defReligion);

                return new
                {
                    Cultures = c,
                    Religions = r,
                    DefaultCulture = defCulture,
                    DefaultReligion = defReligion
                };
            });

            Cultures.Clear();
            foreach (var c in result.Cultures) Cultures.Add(c);

            Religions.Clear();
            foreach (var r in result.Religions) Religions.Add(r);

            // 仅当用户还没有手动输入时才填充默认值
            if (string.IsNullOrEmpty(Culture) && !string.IsNullOrEmpty(result.DefaultCulture))
                Culture = result.DefaultCulture;
            if (string.IsNullOrEmpty(Religion) && !string.IsNullOrEmpty(result.DefaultReligion))
                Religion = result.DefaultReligion;

            IsLoadingLists = false;
        }
    }
}
