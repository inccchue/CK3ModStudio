using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace WpfPrismFrameworkTemplate.Model
{
    public class LifeEventTemplateSelector : DataTemplateSelector
    {
        public DataTemplate LifeEventTemplate { get; set; }
        public DataTemplate MarriageEventTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is MarriageEvent)
                return MarriageEventTemplate;
            return LifeEventTemplate;
        }
    }
    public class LifeEventFactory
    {
        /// <summary>
        /// 创建生命事件对象
        /// </summary>
        /// <param name="eventType">事件类型</param>
        /// <param name="eventDate">事件日期</param>
        /// <param name="additionalData">额外数据（如婚姻事件的配偶信息）</param>
        /// <returns>对应类型的生命事件对象</returns>
        public static LifeEvent CreateLifeEvent(LifeEventType eventType)
        {
            switch (eventType)
            {
                case LifeEventType.Marriage:
                    return new MarriageEvent(eventType);

                case LifeEventType.Birth:
                case LifeEventType.Death:
                default:
                    return new LifeEvent(eventType);
            }
        }
    }

    public enum LifeEventType
    {
        [Description("出生")]
        Birth,
        [Description("死亡")]
        Death,
        [Description("结婚")]
        Marriage
    }

    public class LifeEvent : BindableBase, IComparable<LifeEvent>, IEquatable<LifeEvent>
    {
        private LifeEventType _eventType;
        private string _eventDate;

        // 构造函数
        public LifeEvent(LifeEventType eventType)
        {
            EventType = eventType;
        }

        public LifeEvent() { }

        // 事件类型
        [DisplayName("事件类型")]
        [Description("事件类型")]
        public LifeEventType EventType
        {
            get { return _eventType; }
            set { SetProperty(ref _eventType, value); }
        }

        // 事件日期（时间字符串）
        [DisplayName("事件日期")]
        [Description("日期格式为 yyyy.MM.dd，年份范围0-9999，月份范围1-12，日范围1-31")]
        public string EventDate
        {
            get { return _eventDate; }
            set
            {
                if (ValidateDateFormat(value))
                {
                    SetProperty(ref _eventDate, value);
                }
                else
                {
                    MessageBox.Show("日期格式不正确");
                }
            }
        }

        // 获取可比较的数值表示
        public long GetDateValue()
        {
            if (string.IsNullOrEmpty(EventDate))
                return 0;

            string[] parts = EventDate.Split('.');
            if (parts.Length != 3)
                return 0;

            if (!int.TryParse(parts[0], out int year) ||
                !int.TryParse(parts[1], out int month) ||
                !int.TryParse(parts[2], out int day))
                return 0;

            // 使用一个大数来表示日期，格式为：年*10000 + 月*100 + 日
            return year * 10000L + month * 100L + day;
        }

        public static int CompareEventDates(LifeEvent date1, LifeEvent date2)
        {
            if (date1 == null && date2 == null)
                return 0;
            if (date1 == null)
                return -1;
            if (date2 == null)
                return 1;

            long value1 = date1.GetDateValue();
            long value2 = date2.GetDateValue();
            return value1.CompareTo(value2);
        }

        // 比较当前日期与另一个日期
        public int CompareTo(LifeEvent other)
        {
            if (other == null)
                return 1;
            return GetDateValue().CompareTo(other.GetDateValue());
        }

        private bool ValidateDateFormat(string dateString)
        {
            if (string.IsNullOrEmpty(dateString))
                return false;

            string[] parts = dateString.Split('.');
            if (parts.Length != 3)
                return false;

            // 验证年份 (0-9999)
            if (!int.TryParse(parts[0], out int year) || year < 0 || year > 9999)
                return false;

            // 验证月份 (1-12)
            if (!int.TryParse(parts[1], out int month) || month < 1 || month > 12)
                return false;

            // 验证日 (1-31)
            if (!int.TryParse(parts[2], out int day) || day < 1 || day > 31)
                return false;

            return true;
        }

        // 实现 IEquatable<LifeEvent>
        public bool Equals(LifeEvent other)
        {
            if (other == null)
                return false;

            return GetDateValue() == other.GetDateValue();
        }

        public override bool Equals(object obj)
        {
            return obj is LifeEvent life && Equals(life);
        }

        public override int GetHashCode()
        {
            return GetDateValue().GetHashCode();
        }

        // 重载运算符 >
        public static bool operator >(LifeEvent left, LifeEvent right)
        {
            if (left is null)
                return false;
            if (right is null)
                return true;

            return left.GetDateValue() > right.GetDateValue();
        }

        // 重载运算符 
        public static bool operator <(LifeEvent left, LifeEvent right)
        {
            if (left is null)
                return right != null;
            if (right is null)
                return false;

            return left.GetDateValue() < right.GetDateValue();
        }

        // 重载运算符 ==
        public static bool operator ==(LifeEvent left, LifeEvent right)
        {
            if (ReferenceEquals(left, right))
                return true;
            if (left is null || right is null)
                return false;

            return left.GetDateValue() == right.GetDateValue();
        }

        // 重载运算符 !=
        public static bool operator !=(LifeEvent left, LifeEvent right)
        {
            return !(left == right);
        }

        // 重载运算符 >=
        public static bool operator >=(LifeEvent left, LifeEvent right)
        {
            if (left is null)
                return right is null;
            if (right is null)
                return true;

            return left.GetDateValue() >= right.GetDateValue();
        }

        // 重载运算符 <=
        public static bool operator <=(LifeEvent left, LifeEvent right)
        {
            if (left is null)
                return true;
            if (right is null)
                return false;

            return left.GetDateValue() <= right.GetDateValue();
        }

        // 查找最大日期的静态方法
        public static LifeEvent FindMaxDate(IEnumerable<LifeEvent> events)
        {
            if (events == null || !events.Any())
                return null;

            return events.OrderByDescending(e => e.GetDateValue()).FirstOrDefault();
        }

        // 查找最大日期（参数数组版本）
        public static LifeEvent FindMaxDate(params LifeEvent[] events)
        {
            if (events == null || events.Length == 0)
                return null;

            return FindMaxDate(events.AsEnumerable());
        }

        // 查找最小日期的静态方法
        public static LifeEvent FindMinDate(IEnumerable<LifeEvent> events)
        {
            if (events == null || !events.Any())
                return null;

            return events.OrderBy(e => e.GetDateValue()).FirstOrDefault();
        }

        // 查找最小日期（参数数组版本）
        public static LifeEvent FindMinDate(params LifeEvent[] events)
        {
            if (events == null || events.Length == 0)
                return null;

            return FindMinDate(events.AsEnumerable());
        }
    }

    public class MarriageEvent : LifeEvent
    {
        private string _Spouse;

        // 构造函数
        public MarriageEvent(LifeEventType eventType)
        {
            EventType = eventType;
        }

        public MarriageEvent() { }

        [Description("配偶")]
        [DisplayName("配偶")]
        public string Spouse
        {
            get { return _Spouse; }
            set { SetProperty(ref _Spouse, value); }
        }
    }
}
