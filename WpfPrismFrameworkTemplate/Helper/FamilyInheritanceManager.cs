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
        public People FindNextHeir(People current, ObservableCollection<People> allMembers, string currentDate, HashSet<string> visitedPeople = null)
        {
            // 初始化已访问人员集合，防止无限递归
            if (visitedPeople == null)
                visitedPeople = new HashSet<string>();

            // 如果当前人已经被访问过，直接返回 null 避免循环
            if (current == null || visitedPeople.Contains(current.IdName))
                return null;

            // 标记当前人已被访问
            visitedPeople.Add(current.IdName);

            // 一、检查其子女，男性优先
            if (current.Children != null && current.Children.Count > 0)
            {
                // 1. 先找存活的男性子女（按年龄排序）
                var maleChildren = current.Children
                    .Where(c => c.GetBirthDate() != null &&
                               CompareDates(c.GetBirthDate(), currentDate) <= 0 &&
                               (c.GetDeathDate() == null || CompareDates(currentDate, c.GetDeathDate()) < 0))
                    .Where(c => c.IsMale())
                    .OrderBy(c => c.GetBirthDate(), Comparer<string>.Create((a, b) => CompareDates(a, b)))
                    .ToList();

                if (maleChildren.Any())
                    return maleChildren.First();

                // 2. 如果没有男性子女，找存活的女性子女（按年龄排序）
                var femaleChildren = current.Children
                    .Where(c => c.GetBirthDate() != null &&
                               CompareDates(c.GetBirthDate(), currentDate) <= 0 &&
                               (c.GetDeathDate() == null || CompareDates(currentDate, c.GetDeathDate()) < 0))
                    .Where(c => !c.IsMale())
                    .OrderBy(c => c.GetBirthDate(), Comparer<string>.Create((a, b) => CompareDates(a, b)))
                    .ToList();

                if (femaleChildren.Any())
                    return femaleChildren.First();

                // 3. 检查男性子女的后代（递归检查）
                foreach (var maleChild in current.Children.Where(c => c.IsMale())
                         .OrderBy(c => c.GetBirthDate(), Comparer<string>.Create((a, b) => CompareDates(a, b))))
                {
                    if (maleChild.IsDead() && CompareDates(maleChild.GetDeathDate(), currentDate) <= 0)
                    {
                        var heir = FindNextHeir(maleChild, allMembers, currentDate, visitedPeople);
                        if (heir != null)
                            return heir;
                    }
                }

                // 4. 检查女性子女的后代（递归检查）
                foreach (var femaleChild in current.Children.Where(c => !c.IsMale())
                         .OrderBy(c => c.GetBirthDate(), Comparer<string>.Create((a, b) => CompareDates(a, b))))
                {
                    if (femaleChild.IsDead() && CompareDates(femaleChild.GetDeathDate(), currentDate) <= 0)
                    {
                        var heir = FindNextHeir(femaleChild, allMembers, currentDate, visitedPeople);
                        if (heir != null)
                            return heir;
                    }
                }
            }

            // 二、如果没有子女或子女后代，检查兄弟姐妹及其后代
            if (current.Dad != null)
            {
                var siblings = allMembers.Where(m => m != current && m.Dad == current.Dad).ToList();

                // 1. 先找存活的兄弟（按年龄排序）
                var maleSiblings = siblings
                    .Where(s => s.IsMale() &&
                              s.GetBirthDate() != null &&
                              CompareDates(s.GetBirthDate(), currentDate) <= 0 &&
                              (s.GetDeathDate() == null || CompareDates(currentDate, s.GetDeathDate()) < 0))
                    .OrderBy(s => s.GetBirthDate(), Comparer<string>.Create((a, b) => CompareDates(a, b)))
                    .ToList();

                if (maleSiblings.Any())
                    return maleSiblings.First();

                // 2. 如果没有存活的兄弟，找存活的姐妹（按年龄排序）
                var femaleSiblings = siblings
                    .Where(s => !s.IsMale() &&
                              s.GetBirthDate() != null &&
                              CompareDates(s.GetBirthDate(), currentDate) <= 0 &&
                              (s.GetDeathDate() == null || CompareDates(currentDate, s.GetDeathDate()) < 0))
                    .OrderBy(s => s.GetBirthDate(), Comparer<string>.Create((a, b) => CompareDates(a, b)))
                    .ToList();

                if (femaleSiblings.Any())
                    return femaleSiblings.First();

                // 3. 检查已故兄弟的后代
                foreach (var maleSibling in siblings.Where(s => s.IsMale())
                         .OrderBy(s => s.GetBirthDate(), Comparer<string>.Create((a, b) => CompareDates(a, b))))
                {
                    if (maleSibling.IsDead() && CompareDates(maleSibling.GetDeathDate(), currentDate) <= 0)
                    {
                        var heir = FindNextHeir(maleSibling, allMembers, currentDate, visitedPeople);
                        if (heir != null)
                            return heir;
                    }
                }

                // 4. 检查已故姐妹的后代
                foreach (var femaleSibling in siblings.Where(s => !s.IsMale())
                         .OrderBy(s => s.GetBirthDate(), Comparer<string>.Create((a, b) => CompareDates(a, b))))
                {
                    if (femaleSibling.IsDead() && CompareDates(femaleSibling.GetDeathDate(), currentDate) <= 0)
                    {
                        var heir = FindNextHeir(femaleSibling, allMembers, currentDate, visitedPeople);
                        if (heir != null)
                            return heir;
                    }
                }

                // 5. 如果兄弟姐妹及其后代都没有合适继承人，向上递归找父系亲属
                // 先检查父亲是否已经被访问过，避免无限递归
                if (!visitedPeople.Contains(current.Dad.IdName))
                {
                    return FindNextHeir(current.Dad, allMembers, currentDate, visitedPeople);
                }
            }

            // 三、如果以上都找不到继承人，就找不到合适继承人
            return null;
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
                People nextHolder = FindNextHeir(currentHolder, familyMembers, currentDate, new HashSet<string>());
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
