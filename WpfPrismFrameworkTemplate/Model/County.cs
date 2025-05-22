using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;

namespace WpfPrismFrameworkTemplate.Model
{
    public class County : BindableBase
    {
        public string Name { get; set; }
        private ObservableCollection<HolderEntry> _HolderEntries = new ObservableCollection<HolderEntry>();
        private ObservableCollection<LiegeEntry> _LiegeEntries = new ObservableCollection<LiegeEntry>();
        public ObservableCollection<OtherEntry> OtherEntries { get; set; }

        public County()
        {
            OtherEntries = new ObservableCollection<OtherEntry>();
        }

        public ObservableCollection<HolderEntry> HolderEntries
        {
            get => _HolderEntries;
            set => SetProperty(ref _HolderEntries, value);
        }
        public ObservableCollection<LiegeEntry> LiegeEntries
        {
            get => _LiegeEntries;
            set => SetProperty(ref _LiegeEntries, value);
        }
        public override string ToString()
        {
            return Name;
        }
    }

    public class HolderEntry
    {
        public string StartDate { get; set; }
        public string Holder { get; set; }
    }

    public class LiegeEntry
    {
        public string StartDate { get; set; }
        public string Liege { get; set; }
    }

    public class OtherEntry
    {
        public string StartDate { get; set; }
        public string Content { get; set; }
    }
}
