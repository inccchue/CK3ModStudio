using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfPrismFrameworkTemplate.Model;

namespace WpfPrismFrameworkTemplate.Helper
{
    public class FamilyInheritanceManager
    {
        // 比较日期字符串
        private int CompareDates(string date1, string date2)
        {
            if (string.IsNullOrEmpty(date1) && string.IsNullOrEmpty(date2))
                return 0;
            if (string.IsNullOrEmpty(date1))
                return 1;
            if (string.IsNullOrEmpty(date2))
                return -1;

            DateTime dt1 = ParseDate(date1);
            DateTime dt2 = ParseDate(date2);

            return dt1.CompareTo(dt2);
        }

        // 解析日期
        private DateTime ParseDate(string dateStr)
        {
            string[] parts = dateStr.Split('.');
            int year = int.Parse(parts[0]);
            int month = int.Parse(parts[1]);
            int day = int.Parse(parts[2]);

            return new DateTime(year, month, day);
        }

        // 找出家族最年长成员
        public People FindOldestMember(ObservableCollection<People> members)
        {
            return members
                .Where(m => m.GetBirthDate() != null)
                .OrderBy(m => m.GetBirthDate(), Comparer<string>.Create((a, b) => CompareDates(a, b)))
                .FirstOrDefault();
        }

        // 根据男性优先继承法找出下一个继承人
        public People FindNextHeir(People current, ObservableCollection<People> allMembers, string currentDate)
        {
            // 首先检查其子女，男性优先
            if (current.Children != null && current.Children.Count > 0)
            {
                // 先找男性子女
                var maleChildren = current.Children
                    .Where(c => c.GetBirthDate() != null &&
                               CompareDates(c.GetBirthDate(), currentDate) <= 0 &&
                               (c.GetDeathDate() == null || CompareDates(currentDate, c.GetDeathDate()) < 0))
                    .Where(c => c.IsMale(allMembers))
                    .OrderBy(c => c.GetBirthDate(), Comparer<string>.Create((a, b) => CompareDates(a, b)))
                    .ToList();

                if (maleChildren.Any())
                    return maleChildren.First();

                // 如果没有男性子女，找女性子女
                var femaleChildren = current.Children
                    .Where(c => c.GetBirthDate() != null &&
                               CompareDates(c.GetBirthDate(), currentDate) <= 0 &&
                               (c.GetDeathDate() == null || CompareDates(currentDate, c.GetDeathDate()) < 0))
                    .Where(c => !c.IsMale(allMembers))
                    .OrderBy(c => c.GetBirthDate(), Comparer<string>.Create((a, b) => CompareDates(a, b)))
                    .ToList();

                if (femaleChildren.Any())
                    return femaleChildren.First();
            }

            // 如果没有合适的子女，查找兄弟姐妹的后代
            // 这部分需要在具体实现中根据家族结构进一步完善

            return null; // 没有找到合适的继承人
        }

        // 更新领地持有者集合
        public ObservableCollection<HolderEntry> UpdateHolderEntries(
            ObservableCollection<HolderEntry> existingEntries,
            ObservableCollection<People> familyMembers)
        {
            // 创建新的结果集合
            var resultEntries = new ObservableCollection<HolderEntry>();

            // 找出家族中最年长成员作为首任领地持有者
            People firstHolder = FindOldestMember(familyMembers);
            if (firstHolder == null)
                return existingEntries; // 如果没有找到有效成员，返回原始集合

            string firstHolderBirthDate = firstHolder.GetBirthDate();

            // 保留那些比家族首任领地持有者出生时间早的其他家族持有者
            foreach (var entry in existingEntries)
            {
                if (CompareDates(entry.StartDate, firstHolderBirthDate) < 0)
                {
                    resultEntries.Add(entry);
                }
                else
                {
                    break; // 一旦找到比首任领地持有者出生日期晚的，就停止添加
                }
            }

            // 添加家族首任领地持有者
            resultEntries.Add(new HolderEntry
            {
                StartDate = firstHolderBirthDate,
                Holder = firstHolder.IdName
            });

            // 构建家族内的继承链
            People currentHolder = firstHolder;
            string currentDate = firstHolder.GetDeathDate();

            while (currentDate != null)
            {
                // 找出下一任继承人
                People nextHolder = FindNextHeir(currentHolder, familyMembers, currentDate);
                if (nextHolder == null)
                    break;

                // 添加下一任继承人
                resultEntries.Add(new HolderEntry
                {
                    StartDate = currentDate,
                    Holder = nextHolder.IdName
                });

                // 更新状态
                currentHolder = nextHolder;
                currentDate = currentHolder.GetDeathDate();
            }

            // 如果有最后一任领地持有者死亡后的其他家族持有者，需要添加回来
            string lastHolderDeathDate = currentDate;
            if (lastHolderDeathDate != null)
            {
                foreach (var entry in existingEntries)
                {
                    if (CompareDates(entry.StartDate, lastHolderDeathDate) >= 0)
                    {
                        resultEntries.Add(entry);
                    }
                }
            }

            return resultEntries;
        }
    }
}
