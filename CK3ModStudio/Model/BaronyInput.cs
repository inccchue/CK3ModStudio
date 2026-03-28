using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WpfPrismFrameworkTemplate.Model
{
    public class BaronyInput : INotifyPropertyChanged
    {
        private string _key = "";
        private int? _provinceId;
        private string _englishName = "";
        private string _chineseName = "";
        private string _buildingLevel = "castle_03";

        public string Key
        {
            get => _key;
            set { _key = value; OnPropertyChanged(); }
        }

        public int? ProvinceId
        {
            get => _provinceId;
            set { _provinceId = value; OnPropertyChanged(); }
        }

        public string EnglishName
        {
            get => _englishName;
            set { _englishName = value; OnPropertyChanged(); }
        }

        public string ChineseName
        {
            get => _chineseName;
            set { _chineseName = value; OnPropertyChanged(); }
        }

        // 建筑等级，如 castle_01 / castle_02 / castle_03
        public string BuildingLevel
        {
            get => _buildingLevel;
            set { _buildingLevel = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
