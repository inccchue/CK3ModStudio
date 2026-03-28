using MySql.Data.EntityFramework;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfPrismFrameworkTemplate.Model;

namespace WpfPrismFrameworkTemplate.Helper
{
    // 1. 首先创建数据库上下文
    [DbConfigurationType(typeof(MySqlEFConfiguration))]  // 让EF6支持MySQL
    public class FamilyDbContext : DbContext
    {
        public DbSet<Family> Families { get; set; }
        public DbSet<People> Peoples { get; set; }

        public FamilyDbContext() : base("name=MySqlConnection")
        {
            // 禁用延迟加载
            this.Configuration.LazyLoadingEnabled = false;
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // 配置Family实体
            modelBuilder.Entity<Family>()
                .ToTable("Families");

            // 配置People实体
            modelBuilder.Entity<People>()
                .ToTable("Peoples");

            // 配置Family和People的一对多关系
            modelBuilder.Entity<Family>()
                .HasMany(f => f.Members)
                .WithOptional()
                .WillCascadeOnDelete(true);

            base.OnModelCreating(modelBuilder);
        }
    }

    public interface IFamilyRepository
    {
        Task<ObservableCollection<Family>> LoadFamiliesAsync();
        Task SaveFamiliesAsync(ObservableCollection<Family> families);
        Task<Family> GetFamilyByNameAsync(string familyName);
        Task AddFamilyAsync(Family family);
        Task UpdateFamilyAsync(Family family);
        Task DeleteFamilyAsync(string familyName);
    }

    // 2. 创建数据仓储类
    public class FamilyRepository : IFamilyRepository, IDisposable
    {
        private readonly FamilyDbContext _context;
        private readonly IEventAggregator _eventAggregator;

        public FamilyRepository(IEventAggregator eventAggregator)
        {
            _context = new FamilyDbContext();
            _eventAggregator = eventAggregator;
        }

        public async Task<ObservableCollection<Family>> LoadFamiliesAsync()
        {
            try
            {
                var families = await _context.Families
                    .Include(f => f.Members)
                    .ToListAsync();

                return new ObservableCollection<Family>(families);
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ErrorOccurredEvent>().Publish(
                    new ErrorEventArgs("加载家族数据失败", ex));
                throw;
            }
        }

        public async Task SaveFamiliesAsync(ObservableCollection<Family> families)
        {
            try
            {
                // 开始新的数据库事务
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        // 清空现有数据
                        _context.Families.RemoveRange(_context.Families);
                        _context.Peoples.RemoveRange(_context.Peoples);

                        // 添加新数据
                        foreach (var family in families)
                        {
                            _context.Families.Add(family);
                        }

                        await _context.SaveChangesAsync();
                        transaction.Commit();

                        // 发布保存成功事件
                        _eventAggregator.GetEvent<DataSavedEvent>().Publish();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ErrorOccurredEvent>().Publish(
                    new ErrorEventArgs("保存家族数据失败", ex));
                throw;
            }
        }

        public async Task<Family> GetFamilyByNameAsync(string familyName)
        {
            try
            {
                return await _context.Families
                    .Include(f => f.Members)
                    .FirstOrDefaultAsync(f => f.FamilyName == familyName);
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ErrorOccurredEvent>().Publish(
                    new ErrorEventArgs($"获取家族 {familyName}, 数据失败", ex));
                throw;
            }
        }

        public async Task AddFamilyAsync(Family family)
        {
            try
            {
                _context.Families.Add(family);
                await _context.SaveChangesAsync();
                _eventAggregator.GetEvent<FamilyAddedEvent>().Publish(family);
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ErrorOccurredEvent>().Publish(
                    new ErrorEventArgs("添加家族失败", ex));
                throw;
            }
        }

        public async Task UpdateFamilyAsync(Family family)
        {
            try
            {
                var existingFamily = await _context.Families
                    .Include(f => f.Members)
                    .FirstOrDefaultAsync(f => f.FamilyName == family.FamilyName);

                if (existingFamily != null)
                {
                    _context.Entry(existingFamily).CurrentValues.SetValues(family);
                    await _context.SaveChangesAsync();
                    _eventAggregator.GetEvent<FamilyUpdatedEvent>().Publish(family);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ErrorOccurredEvent>().Publish(
                    new ErrorEventArgs("更新家族数据失败", ex));
                throw;
            }
        }

        public async Task DeleteFamilyAsync(string familyName)
        {
            try
            {
                var family = await _context.Families
                    .FirstOrDefaultAsync(f => f.FamilyName == familyName);

                if (family != null)
                {
                    _context.Families.Remove(family);
                    await _context.SaveChangesAsync();
                    _eventAggregator.GetEvent<FamilyDeletedEvent>().Publish(familyName);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ErrorOccurredEvent>().Publish(
                    new ErrorEventArgs("删除家族失败", ex));
                throw;
            }
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }

    // 事件定义
    public class DataSavedEvent : PubSubEvent { }
    public class FamilyAddedEvent : PubSubEvent<Family> { }
    public class FamilyUpdatedEvent : PubSubEvent<Family> { }
    public class FamilyDeletedEvent : PubSubEvent<string> { }
    public class ErrorOccurredEvent : PubSubEvent<ErrorEventArgs> { }

    public class ErrorEventArgs
    {
        public string Message { get; }
        public Exception Exception { get; }

        public ErrorEventArgs(string message, Exception exception)
        {
            Message = message;
            Exception = exception;
        }
    }

}
