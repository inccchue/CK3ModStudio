using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Prism.Mvvm;

namespace WpfPrismFrameworkTemplate.Model
{
    public class TimelineEntry: BindableBase
    {
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string Holder { get; set; }
        public Brush Background { get; set; }
        public string DisplayText => $"{StartDate} - {EndDate}: {Holder}";
    }
}
