using Prism.Events;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using ModernWpf.Controls;
using System.ComponentModel;

namespace WpfPrismFrameworkTemplate.Model
{
    public enum SearchType
    {
        [Description("母亲")]
        Mom,
        [Description("父亲")]
        Dad,
        [Description("配偶")]
        Spouse
    }

    public class SaveMessageEvent : PubSubEvent
    {
    }

    public class ParentChangedEventArgs
    {
        public string Text { get; set; }
        public AutoSuggestionBoxTextChangeReason Reason { get; set; }
        public People TargetPeople { get; set; }
        public SearchType SearchType { get; set; }
    }

    public class CountyChangedEventArgs
    {
        public string Text { get; set; }
        public AutoSuggestionBoxTextChangeReason Reason { get; set; }
        public County TargetCounty { get; set; }
    }

    public class ParentChangedEvent : PubSubEvent<ParentChangedEventArgs>
    {
    }

    public class QuerySubmittedEventArgs
    {
        public string QueryText { get; set; }
        public object ChosenSuggestion { get; set; }
        public People TargetPeople { get; set; }
        public SearchType SearchType { get; set; }
    }
    public class CountyQuerySubmittedEventArgs
    {
        public string QueryText { get; set; }
        public object ChosenSuggestion { get; set; }
        public County TargetCounty { get; set; }
    }
    public class QuerySubmittedEvent : PubSubEvent<QuerySubmittedEventArgs>
    {
    }

    public class FileSettingChangeEvent : PubSubEvent<string>
    {
    }
}
