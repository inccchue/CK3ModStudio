using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfPrismFrameworkTemplate.Model
{
    public class County
    {
        public string Name { get; set; }
        public ObservableCollection<HolderEntry> HolderEntries { get; set; }

        public County()
        {
            HolderEntries = new ObservableCollection<HolderEntry>();
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
}
