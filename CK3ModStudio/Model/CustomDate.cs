using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WpfPrismFrameworkTemplate.Model
{

    public class CustomDate : IComparable<CustomDate>, IEquatable<CustomDate>
    {
        public string Value { get; set; }

        public CustomDate(string value)
        {
            if (!IsValidDateFormat(value))
            {
                MessageBox.Show($"日期格式无效: {value}。格式必须为 yyyy.MM.dd，年份范围0-9999，月份范围1-12，日范围1-31");
            }
            else
            {
                Value = value;
            }
                //throw new ArgumentException($"日期格式无效: {value}。格式必须为 yyyy.MM.dd，年份范围0-9999，月份范围1-12，日范围1-31");

            
        }

        public CustomDate() { }

        public override string ToString()
        {
            return Value;
        }

        // 获取日期的数值表示
        private long GetDateValue()
        {
            string[] parts = Value.Split('.');
            int year = int.Parse(parts[0]);
            int month = int.Parse(parts[1]);
            int day = int.Parse(parts[2]);

            return year * 10000L + month * 100L + day;
        }

        // 验证日期格式
        private static bool IsValidDateFormat(string dateString)
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

        // 实现 IComparable<CustomDate> 接口
        public int CompareTo(CustomDate other)
        {
            if (other == null)
                return 1;

            return GetDateValue().CompareTo(other.GetDateValue());
        }

        // 实现 IEquatable<CustomDate> 接口
        public bool Equals(CustomDate other)
        {
            if (other == null)
                return false;

            return GetDateValue() == other.GetDateValue();
        }

        public override bool Equals(object obj)
        {
            return obj is CustomDate date && Equals(date);
        }

        public override int GetHashCode()
        {
            return GetDateValue().GetHashCode();
        }

        // 重载运算符 >
        public static bool operator >(CustomDate left, CustomDate right)
        {
            if (left is null)
                return false;
            if (right is null)
                return true;

            return left.GetDateValue() > right.GetDateValue();
        }

        // 重载运算符 
        public static bool operator <(CustomDate left, CustomDate right)
        {
            if (left == null)
                return right != null;  // 修改为 right != null
            if (right == null)
                return false;

            return left.GetDateValue() < right.GetDateValue();
        }

        // 重载运算符 ==
        public static bool operator ==(CustomDate left, CustomDate right)
        {
            if (left is null)
                return right is null;
            if (right is null)
                return false;

            return left.GetDateValue() == right.GetDateValue();
        }

        // 重载运算符 !=
        public static bool operator !=(CustomDate left, CustomDate right)
        {
            return !(left == right);
        }

        // 重载运算符 >=
        public static bool operator >=(CustomDate left, CustomDate right)
        {
            if (left is null)
                return right is null;
            if (right is null)
                return true;

            return left.GetDateValue() >= right.GetDateValue();
        }

        // 重载运算符 <=
        public static bool operator <=(CustomDate left, CustomDate right)
        {
            if (left is null)
                return true;
            if (right is null)
                return false;

            return left.GetDateValue() <= right.GetDateValue();
        }

        // 查找最大日期
        public static CustomDate Max(params CustomDate[] dates)
        {
            if (dates == null || dates.Length == 0)
                return null;

            return dates.Max();
        }

        // 查找最大日期（列表版本）
        public static CustomDate Max(IEnumerable<CustomDate> dates)
        {
            if (dates == null || !dates.Any())
                return null;

            return dates.Max();
        }

        // 查找最小日期
        public static CustomDate Min(params CustomDate[] dates)
        {
            if (dates == null || dates.Length == 0)
                return null;

            return dates.Min();
        }

        // 查找最小日期（列表版本）
        public static CustomDate Min(IEnumerable<CustomDate> dates)
        {
            if (dates == null || !dates.Any())
                return null;

            return dates.Min();
        }
    }
}
