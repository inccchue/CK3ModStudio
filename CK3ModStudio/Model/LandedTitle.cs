using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace WpfPrismFrameworkTemplate.Model
{
    public enum TitleRank
    {
        Empire,    // e_
        Kingdom,   // k_
        Duchy,     // d_
        County,    // c_
        Barony     // b_
    }

    public class LandedTitle : INotifyPropertyChanged
    {
        private string _key = "";
        private TitleRank _rank;
        private string _capital = "";
        private Color _color = Colors.Gray;
        private int? _province;
        private bool _definiteForm;
        private LandedTitle _parent;
        private bool _isExpanded = false;
        private bool _isSelected;
        private string _canCreate;

        public string Key
        {
            get => _key;
            set { _key = value; OnPropertyChanged(); OnPropertyChanged(nameof(DisplayName)); }
        }

        public TitleRank Rank
        {
            get => _rank;
            set { _rank = value; OnPropertyChanged(); OnPropertyChanged(nameof(RankIcon)); OnPropertyChanged(nameof(RankColor)); OnPropertyChanged(nameof(RankLabel)); OnPropertyChanged(nameof(RankColorBrush)); }
        }

        public string Capital
        {
            get => _capital;
            set { _capital = value; OnPropertyChanged(); }
        }

        public Color Color
        {
            get => _color;
            set { _color = value; OnPropertyChanged(); OnPropertyChanged(nameof(ColorBrush)); }
        }

        public int? Province
        {
            get => _province;
            set { _province = value; OnPropertyChanged(); }
        }

        public bool DefiniteForm
        {
            get => _definiteForm;
            set { _definiteForm = value; OnPropertyChanged(); }
        }

        public string CanCreate
        {
            get => _canCreate;
            set { _canCreate = value; OnPropertyChanged(); }
        }

        public LandedTitle Parent
        {
            get => _parent;
            set { _parent = value; OnPropertyChanged(); }
        }

        public bool IsExpanded
        {
            get => _isExpanded;
            set { _isExpanded = value; OnPropertyChanged(); }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(); }
        }

        public ObservableCollection<LandedTitle> Children { get; } = new ObservableCollection<LandedTitle>();

        public string DisplayName => Key;

        public string RankIcon
        {
            get
            {
                switch (Rank)
                {
                    case TitleRank.Empire: return "👑";
                    case TitleRank.Kingdom: return "🏰";
                    case TitleRank.Duchy: return "⚜";
                    case TitleRank.County: return "🏛";
                    case TitleRank.Barony: return "🏠";
                    default: return "?";
                }
            }
        }

        public string RankLabel
        {
            get
            {
                switch (Rank)
                {
                    case TitleRank.Empire: return "帝国";
                    case TitleRank.Kingdom: return "王国";
                    case TitleRank.Duchy: return "公国";
                    case TitleRank.County: return "伯国";
                    case TitleRank.Barony: return "男爵领";
                    default: return "未知";
                }
            }
        }

        public SolidColorBrush ColorBrush => new SolidColorBrush(Color);

        public Color RankColor
        {
            get
            {
                switch (Rank)
                {
                    case TitleRank.Empire: return Color.FromRgb(148, 0, 211);
                    case TitleRank.Kingdom: return Color.FromRgb(220, 20, 60);
                    case TitleRank.Duchy: return Color.FromRgb(30, 144, 255);
                    case TitleRank.County: return Color.FromRgb(34, 139, 34);
                    case TitleRank.Barony: return Color.FromRgb(139, 90, 43);
                    default: return Color.FromRgb(128, 128, 128);
                }
            }
        }

        public SolidColorBrush RankColorBrush => new SolidColorBrush(RankColor);

        public void AddChild(LandedTitle child)
        {
            child.Parent = this;
            Children.Add(child);
        }

        public bool RemoveChild(LandedTitle child)
        {
            if (Children.Remove(child))
            {
                child.Parent = null;
                return true;
            }
            return false;
        }

        public static bool MoveTo(LandedTitle title, LandedTitle newParent)
        {
            if (title.Parent == null) return false;
            if (title == newParent) return false;
            if (IsAncestor(newParent, title)) return false;

            var oldParent = title.Parent;
            oldParent.RemoveChild(title);
            newParent.AddChild(title);
            return true;
        }

        private static bool IsAncestor(LandedTitle candidate, LandedTitle potentialAncestor)
        {
            var current = candidate.Parent;
            while (current != null)
            {
                if (current == potentialAncestor) return true;
                current = current.Parent;
            }
            return false;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public override string ToString() => Key;
    }
}
