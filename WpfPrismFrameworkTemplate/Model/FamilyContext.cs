using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfPrismFrameworkTemplate.Model
{
    public class FamilyModel
    {
        public int Id { get; set; }
        public string FamilyName { get; set; }

        // 确保 Members 是 ICollection 类型，且标记为 virtual 以支持延迟加载
        public virtual ICollection<PeopleModel> Members { get; set; }
    }

    public class PeopleModel
    {
        public int Id { get; set; }
        public string IdName { get; set; }
        public string Name { get; set; }
        public string Dynasty { get; set; }
        public string Religion { get; set; }
        public string Culture { get; set; }
        public string BirthDay { get; set; }
        public string DeathDay { get; set; }

        public int FamilyId { get; set; }  // 外键
        public virtual FamilyModel Family { get; set; } // 导航属性
    }
    public class FamilyDbContext : DbContext
    {
        public FamilyDbContext() : base("name=MySqlConnection") { }

        public DbSet<FamilyModel> Families { get; set; }
        public DbSet<PeopleModel> People { get; set; }
    }

}
