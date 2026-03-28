using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfPrismFrameworkTemplate.Helper
{
    public static class CommonHelper
    {
        public static DateTime ParseDate(string dateString)
        {
            if (DateTime.TryParseExact(dateString, "yyyy.M.d", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out DateTime date))
            {
                return date;
            }

            // 解析失败时返回最小日期，这样会排在最前面
            return DateTime.MinValue;
        }
    }
}
