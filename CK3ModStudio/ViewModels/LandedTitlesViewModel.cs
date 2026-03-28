using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Prism.Mvvm;
using Prism.Regions;
using WpfPrismFrameworkTemplate.Helper;
using WpfPrismFrameworkTemplate.Model;

namespace WpfPrismFrameworkTemplate.ViewModels
{
    public class LandedTitlesViewModel : BindableBase, INavigationAware
    {
        private readonly LandedTitlesParser _parser = new LandedTitlesParser();
        private LandedTitle _selectedTitle;
        private LandedTitle _dragSource;
        private string _statusMessage = "就绪";
        private string _searchText = "";
        private string _currentFilePath;
        private bool _isDirty;

        public ObservableCollection<LandedTitle> RootTitles { get; } = new ObservableCollection<LandedTitle>();
        public ObservableCollection<LandedTitle> FilteredTitles { get; } = new ObservableCollection<LandedTitle>();

        public LandedTitle SelectedTitle
        {
            get => _selectedTitle;
            set { SetProperty(ref _selectedTitle, value); RaisePropertyChanged(nameof(HasSelection)); }
        }

        public LandedTitle DragSource
        {
            get => _dragSource;
            set => SetProperty(ref _dragSource, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                SetProperty(ref _searchText, value);
                ApplyFilter();
            }
        }

        public string CurrentFilePath
        {
            get => _currentFilePath;
            set { SetProperty(ref _currentFilePath, value); RaisePropertyChanged(nameof(WindowTitle)); }
        }

        public bool IsDirty
        {
            get => _isDirty;
            set { SetProperty(ref _isDirty, value); RaisePropertyChanged(nameof(WindowTitle)); }
        }

        public bool HasSelection => SelectedTitle != null;

        public string WindowTitle =>
            string.Format("法理编辑器 - {0}{1}",
                CurrentFilePath != null ? Path.GetFileName(CurrentFilePath) : "未命名",
                IsDirty ? " *" : "");

        public void LoadFile(string path)
        {
            var titles = _parser.ParseFile(path);
            PopulateTitles(titles);
            CurrentFilePath = path;
            IsDirty = false;
            StatusMessage = string.Format("已加载: {0}，共 {1} 个法理", Path.GetFileName(path), CountAll());
        }

        public async Task LoadFileAsync(string path)
        {
            StatusMessage = "正在加载...";
            var titles = await Task.Run(() => _parser.ParseFile(path));
            PopulateTitles(titles);
            CurrentFilePath = path;
            IsDirty = false;
            StatusMessage = string.Format("已加载: {0}，共 {1} 个法理", Path.GetFileName(path), CountAll());
            AppSettings.LastLandedTitlesFile = path;
        }

        public string GetLastFilePath() => AppSettings.LastLandedTitlesFile;

        private void PopulateTitles(List<LandedTitle> titles)
        {
            RootTitles.Clear();
            FilteredTitles.Clear();
            foreach (var t in titles)
            {
                RootTitles.Add(t);
                FilteredTitles.Add(t);
            }
        }

        public void LoadContent(string content)
        {
            var titles = _parser.ParseContent(content);
            PopulateTitles(titles);
            IsDirty = false;
            StatusMessage = string.Format("已加载内容，共 {0} 个法理", CountAll());
        }

        public void SaveFile(string path)
        {
            var content = _parser.Serialize(RootTitles);
            File.WriteAllText(path, content);
            CurrentFilePath = path;
            IsDirty = false;
            StatusMessage = string.Format("已保存: {0}", Path.GetFileName(path));
        }

        public bool MoveTitle(LandedTitle title, LandedTitle newParent)
        {
            if ((int)title.Rank <= (int)newParent.Rank)
            {
                StatusMessage = string.Format("错误: 无法将 {0} 移至 {1} 下 (法理等级不匹配)", title.RankLabel, newParent.RankLabel);
                return false;
            }

            if (IsAncestor(newParent, title))
            {
                StatusMessage = "错误: 不能将父级移入其子级";
                return false;
            }

            var oldParent = title.Parent;
            if (oldParent != null)
                oldParent.Children.Remove(title);
            else
                RootTitles.Remove(title);

            title.Parent = newParent;
            newParent.Children.Add(title);
            IsDirty = true;
            StatusMessage = string.Format("已将 [{0}] 移至 [{1}]", title.Key, newParent.Key);
            return true;
        }

        private bool IsAncestor(LandedTitle candidate, LandedTitle potentialAncestor)
        {
            var cur = candidate.Parent;
            while (cur != null)
            {
                if (cur == potentialAncestor) return true;
                cur = cur.Parent;
            }
            return false;
        }

        private void ApplyFilter()
        {
            FilteredTitles.Clear();
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                foreach (var t in RootTitles) FilteredTitles.Add(t);
                // Do NOT auto-expand — let user expand manually to avoid rendering all nodes
                return;
            }

            foreach (var t in RootTitles)
            {
                if (FilterTitle(t, SearchText.ToLower()))
                    FilteredTitles.Add(t);
            }
        }

        private bool FilterTitle(LandedTitle title, string filter)
        {
            bool selfMatch = title.Key.ToLower().Contains(filter);
            bool childMatch = false;
            foreach (var c in title.Children)
                if (FilterTitle(c, filter)) { childMatch = true; break; }
            title.IsExpanded = childMatch || selfMatch;
            return selfMatch || childMatch;
        }

        private void SetVisible(LandedTitle t, bool expanded)
        {
            t.IsExpanded = expanded;
            foreach (var c in t.Children) SetVisible(c, expanded);
        }

        private int CountAll()
        {
            int count = 0;
            foreach (var t in RootTitles) count += CountTitle(t);
            return count;
        }

        private int CountTitle(LandedTitle t)
        {
            int c = 1;
            foreach (var child in t.Children) c += CountTitle(child);
            return c;
        }

        public void AddTitle(LandedTitle newTitle, LandedTitle parent)
        {
            if (parent != null)
            {
                newTitle.Parent = parent;
                parent.Children.Add(newTitle);
                parent.IsExpanded = true;
            }
            else
            {
                RootTitles.Add(newTitle);
                FilteredTitles.Add(newTitle);
            }
            IsDirty = true;
            SelectedTitle = newTitle;
            StatusMessage = string.Format("已新增: [{0}]{1}", newTitle.Key,
                parent != null ? string.Format(" → [{0}]", parent.Key) : " (根级)");
            ApplyFilter();
        }

        public List<LandedTitle> GetPotentialParents(TitleRank childRank)
        {
            if (childRank == TitleRank.Empire) return new List<LandedTitle>();
            var parentRank = (TitleRank)((int)childRank - 1);
            return GetTitlesOfRank(parentRank);
        }

        public IEnumerable<LandedTitle> GetAllTitles()
        {
            foreach (var t in RootTitles)
                foreach (var flat in Flatten(t))
                    yield return flat;
        }

        private IEnumerable<LandedTitle> Flatten(LandedTitle t)
        {
            yield return t;
            foreach (var c in t.Children)
                foreach (var f in Flatten(c))
                    yield return f;
        }

        public List<LandedTitle> GetTitlesOfRank(TitleRank rank)
        {
            var result = new List<LandedTitle>();
            foreach (var t in RootTitles) CollectByRank(t, rank, result);
            return result;
        }

        private void CollectByRank(LandedTitle t, TitleRank rank, List<LandedTitle> result)
        {
            if (t.Rank == rank) result.Add(t);
            foreach (var c in t.Children) CollectByRank(c, rank, result);
        }

        public void OnNavigatedTo(NavigationContext navigationContext) { }

        public bool IsNavigationTarget(NavigationContext navigationContext) => true;

        public void OnNavigatedFrom(NavigationContext navigationContext) { }
    }
}
