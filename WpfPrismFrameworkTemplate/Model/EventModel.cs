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

    public enum UpdateContentType
    {
        [Description("家族更新")]
        UpdateFamily,
        [Description("单个家族更新")]
        UpdateSingleFamily,
        [Description("单个家族删除")]
        RemoveSingleFamily,
        [Description("角色更新")]
        UpdateCharacter,
        [Description("单个角色更新")]
        UpdateSingleCharacter,
        [Description("单个角色删除")]
        RemoveSingleCharacter,
    }

    public enum MsgLevel
    {
        [Description("普通消息")]
        Normal,
        [Description("成功消息")]
        Success,
        [Description("警告消息")]
        Warning,
        [Description("报警消息")]
        Alarm,
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

    public class SelectFamilyChangeEvent : PubSubEvent<Family>
    {
    }

    public class UpdateContentEventArgs
    {
        public UpdateContentType type { get; set; }
        public object value { get; set; }
    }
    public class UpdateContentEvent : PubSubEvent<UpdateContentEventArgs>
    {
    }

    public class DebugMsgEventArgs
    {
        public MsgLevel level { get; set; }
        public string content { get; set; }
        public DateTime timestamp { get; set; }
    }
    public class DebugMsgEvent : PubSubEvent<DebugMsgEventArgs>
    {
    }
}
