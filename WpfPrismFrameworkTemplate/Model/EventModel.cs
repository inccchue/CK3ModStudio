using Prism.Events;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using ModernWpf.Controls;

namespace WpfPrismFrameworkTemplate.Model
{
    public class SaveMessageEvent : PubSubEvent
    {
    }

    public class ParentChangedEventArgs
    {
        public string Text { get; set; }
        public AutoSuggestionBoxTextChangeReason Reason { get; set; }
        public bool IsMom { get; set; }
    }
    
    public class ParentChangedEvent : PubSubEvent<ParentChangedEventArgs>
    {
    }

    public class QuerySubmittedEventArgs
    {
        public string QueryText { get; set; }
        public object ChosenSuggestion { get; set; }
        public People TargetPeople { get; set; }
        public bool IsMom { get; set; }
    }
    public class QuerySubmittedEvent : PubSubEvent<QuerySubmittedEventArgs>
    {
    }
}
