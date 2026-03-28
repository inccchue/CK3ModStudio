using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using WpfPrismFrameworkTemplate.Model;

namespace WpfPrismFrameworkTemplate.ViewModels
{
    public class MoveLandedTitleViewModel : INotifyPropertyChanged
    {
        private LandedTitle _selectedTarget;
        private string _filterText = "";

        public LandedTitle SourceTitle { get; }
        public ObservableCollection<LandedTitle> AvailableTargets { get; } = new ObservableCollection<LandedTitle>();
        public ObservableCollection<LandedTitle> FilteredTargets { get; } = new ObservableCollection<LandedTitle>();

        public LandedTitle SelectedTarget
        {
            get => _selectedTarget;
            set { _selectedTarget = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanConfirm)); }
        }

        public string FilterText
        {
            get => _filterText;
            set { _filterText = value; OnPropertyChanged(); ApplyFilter(); }
        }

        public bool CanConfirm => SelectedTarget != null;

        public string Title => string.Format("移动 [{0}] ({1})", SourceTitle.Key, SourceTitle.RankLabel);

        public string Instruction
        {
            get
            {
                switch (SourceTitle.Rank)
                {
                    case TitleRank.Barony: return "选择目标伯国 (c_)";
                    case TitleRank.County: return "选择目标公国 (d_)";
                    case TitleRank.Duchy: return "选择目标王国 (k_)";
                    case TitleRank.Kingdom: return "选择目标帝国 (e_)";
                    default: return "选择目标";
                }
            }
        }

        public MoveLandedTitleViewModel(LandedTitle source, IEnumerable<LandedTitle> targets)
        {
            SourceTitle = source;
            foreach (var t in targets) AvailableTargets.Add(t);
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            FilteredTargets.Clear();
            foreach (var t in AvailableTargets)
            {
                if (string.IsNullOrWhiteSpace(FilterText) ||
                    t.Key.IndexOf(FilterText, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    FilteredTargets.Add(t);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
