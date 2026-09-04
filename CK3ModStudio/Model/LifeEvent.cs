using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.Collections.ObjectModel;
using System.Globalization;
using GongSolutions.Wpf.DragDrop;
using WpfPrismFrameworkTemplate.Helper;

namespace WpfPrismFrameworkTemplate.Model
{
    public class LifeEventFactory
    {
        /// <summary>
        /// 创建生命事件对象
        /// </summary>
        /// <param name="eventType">事件类型</param>
        /// <param name="eventDate">事件日期</param>
        /// <param name="additionalData">额外数据（예婚姻事件的배우자信息）</param>
        /// <returns>对应类型的生命事件对象</returns>
        public static LifeEvent CreateLifeEvent(LifeEventType eventType)
        {
            switch (eventType)
            {
                case LifeEventType.Marriage:
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
        [Description("日期格式为 yyyy.MM.dd，이어야 합니다 (연도 0-9999, 월 1-12, 일 1-31)")]
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
                    MessageBox.Show("날짜 형식이 올바르지 않습니다");
                }
            }
        }

        public static void RandomCreateBirthAndDeath(People targetPeople)
        {
            // 예果没有父母信息，直接返回
            if (targetPeople.Dad == null && targetPeople.Mom == null)
            {
                return;
            }

            Random random = new Random();
            DateTime? minBirthDate = null;
            DateTime? maxBirthDate = null;

            // 处理아버지的约束条件
            if (targetPeople.Dad != null)
            {
                DateTime? dadBirthDate = GetLifeEventDate(targetPeople.Dad, LifeEventType.Birth);
                DateTime? dadDeathDate = GetLifeEventDate(targetPeople.Dad, LifeEventType.Death);

                if (dadBirthDate.HasValue)
                {
                    // 아버지至少16세才能生孩하위
                    DateTime dadMinParentAge = dadBirthDate.Value.AddYears(16);
                    minBirthDate = dadMinParentAge;
                }

                if (dadDeathDate.HasValue)
                {
                    if(dadBirthDate.HasValue && dadBirthDate.Value > dadDeathDate.Value)
                    {
                        // 예果아버지的出生日期晚于死亡日期，直接返回
                        return;
                    }
                    if(dadBirthDate.HasValue&& (dadBirthDate.Value.AddYears(45)<= dadDeathDate))
                    {
                        maxBirthDate = dadBirthDate.Value.AddYears(45);
                    }
                    else
                    {
                        // 孩하위最晚出生日期不能超过아버지死亡
                        maxBirthDate = dadDeathDate.Value;
                    }
                        
                }
            }

            // 处理어머니的约束条件
            if (targetPeople.Mom != null)
            {
                DateTime? momBirthDate = GetLifeEventDate(targetPeople.Mom, LifeEventType.Birth);
                DateTime? momDeathDate = GetLifeEventDate(targetPeople.Mom, LifeEventType.Death);

                if (momBirthDate.HasValue)
                {
                    // 어머니至少15세才能生孩하위
                    DateTime momMinParentAge = momBirthDate.Value.AddYears(15);

                    // 更新最小出生日期（取父母中的较大值）
                    if (minBirthDate == null || momMinParentAge > minBirthDate)
                    {
                        minBirthDate = momMinParentAge;
                    }
                }

                if (momDeathDate.HasValue)
                {
                    if (momBirthDate.HasValue && momBirthDate.Value > momDeathDate.Value)
                    {
                        return;
                    }
                    if (maxBirthDate == null || momDeathDate.Value < maxBirthDate)
                    {
                        if (momBirthDate.HasValue && (momBirthDate.Value.AddYears(40) <= momDeathDate))
                        {
                            if (momBirthDate.Value.AddYears(40) < maxBirthDate)
                            {
                                maxBirthDate = momBirthDate.Value.AddYears(40);
                            }
                            
                        }
                        else
                        {
                            if(momDeathDate.Value < maxBirthDate)
                            {
                                maxBirthDate = momDeathDate.Value;
                            }
                        }
                    }
                    
                    
                }
            }

            // 예果没有有效的出生날짜 범위，就不생성了
            if (!minBirthDate.HasValue || !maxBirthDate.HasValue || minBirthDate > maxBirthDate)
            {
                return;
            }

            // 在有效范围内随机생성出生日期
            DateTime birthDate = RandomDateBetween(random, minBirthDate.Value, maxBirthDate.Value);

            // 添加出生事件
            AddLifeEvent(targetPeople, LifeEventType.Birth, birthDate);

            // 생성符合평균 수명为45세的正态分布的死亡日期，范围在15-70세之间
            double meanAge = 45.0; // 평균 수명
            double stdDev = 12.0;  // 标准差 - 使得大部分值落在15-70范围内

            // 使用正态分布생성年龄
            int ageInYears;
            do
            {
                // 正态分布随机数
                ageInYears = (int)Math.Round(NormalDistribution(random, meanAge, stdDev));
            } while (ageInYears < 15 || ageInYears > 70); // 确保年龄在15-70세之间

            // 计算死亡日期
            DateTime deathDate = birthDate.AddYears(ageInYears);

            // 在一年内随机添加天数，使死亡日期更自然
            deathDate = deathDate.AddDays(random.Next(365));

            // 添加死亡事件
            AddLifeEvent(targetPeople, LifeEventType.Death, deathDate);
        }

        // 获取指定类型的生命事件日期
        private static DateTime? GetLifeEventDate(People people, LifeEventType eventType)
        {
            if (people.LifeEventList == null)
                return null;

            var lifeEvent = people.LifeEventList.FirstOrDefault(e => e.EventType == eventType);
            if (lifeEvent == null)
                return null;

            return CommonHelper.ParseDate(lifeEvent.EventDate);
        }

        // 在两个日期之间随机생성一个日期，前5年概率最大，然后概率逐渐降低
        private static DateTime RandomDateBetween(Random random, DateTime start, DateTime end)
        {
            int range = (end - start).Days;
            if (range <= 0)
                return start;

            // 确定5年的天数
            int fiveYearsInDays = 5 * 365;

            // 예果总范围小于5年，则使用有偏向起点的分布
            if (range <= fiveYearsInDays)
            {
                // 使用三角分布，偏向于起始点
                double triangularRandom = TriangularDistribution(random, 0, range, 0);
                return start.AddDays((int)triangularRandom);
            }
            else
            {
                // 优先考虑5年内的范围，使用指数衰减
                // 생성一个0까지1的随机数
                double randomValue = random.NextDouble();

                // 应用指数分布公式: -ln(1-r)/lambda
                // lambda控制衰减速率，较小的lambda意味着更快的衰减
                double lambda = 1.0 / (range * 0.25); // 调整参数使分布合理

                // 防止ln(0)
                if (randomValue >= 0.999)
                    randomValue = 0.999;

                // 计算随机天数（使用指数分布）
                double expRandom = -Math.Log(1 - randomValue) / lambda;

                // 将值限制在合法范围内
                int randomDays = (int)Math.Min(expRandom, range);

                return start.AddDays(randomDays);
            }
        }

        // 三角分布随机数생성
        private static double TriangularDistribution(Random random, double min, double max, double mode)
        {
            double u = random.NextDouble();
            double normalizedMode = (mode - min) / (max - min);

            if (u <= normalizedMode)
                return min + Math.Sqrt(u * (max - min) * (mode - min));
            else
                return max - Math.Sqrt((1 - u) * (max - min) * (max - mode));
        }

        // 생성正态分布的随机数
        private static double NormalDistribution(Random random, double mean, double stdDev)
        {
            // Box-Muller算法생성正态分布随机数
            double u1 = 1.0 - random.NextDouble(); // 避免取까지0
            double u2 = 1.0 - random.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
            return mean + stdDev * randStdNormal;
        }

        // 添加生命事件
        private static void AddLifeEvent(People people, LifeEventType eventType, DateTime date)
        {
            if (people.LifeEventList == null)
            {
                people.LifeEventList = new ObservableCollection<LifeEvent>();
            }

            // 检查是否已经存在同类型的事件
            var existingEvent = people.LifeEventList.FirstOrDefault(e => e.EventType == eventType);
            if (existingEvent != null)
            {
                // 移除已存在的事件
                people.LifeEventList.Remove(existingEvent);
            }

            // 格式化日期为指定格式 (yyyy.M.d)
            string formattedDate = date.ToString("yyyy.M.d");

            // 创建并添加新事件
            var newEvent = new LifeEvent
            {
                EventType = eventType,
                EventDate = formattedDate
            };

            people.LifeEventList.Add(newEvent);
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
}
