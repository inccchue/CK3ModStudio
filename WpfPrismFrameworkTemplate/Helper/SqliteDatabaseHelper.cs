using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.SQLite;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Windows.System;
using WpfPrismFrameworkTemplate.Model;

namespace WpfPrismFrameworkTemplate.Helper
{
    // DbContext 配置
    public class AppDbContext : DbContext
    {
        public AppDbContext(string connectionString)
            : base(new SQLiteConnection(connectionString), true)  // 告诉 EF 用 SQLiteConnection
        {
            Database.SetInitializer<AppDbContext>(null);
        }

        public DbSet<Family> Families { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Family>()
                .HasKey(f => f.FamilyName)
                .Property(f => f.FamilyName)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            modelBuilder.Entity<Family>()
                .Property(f => f.FamilyName_CN)
                .IsRequired()
                .HasMaxLength(100);

            base.OnModelCreating(modelBuilder);
        }
    }

    public class SqliteDatabaseHelper : IDisposable
    {
        private readonly string _connectionString;
        private readonly string _databasePath;
        private AppDbContext _context;

        public SqliteDatabaseHelper(string databasePath)
        {
            _databasePath = databasePath;
            _connectionString = $"Data Source={databasePath};";
            InitializeDatabase();
        }

        private AppDbContext Context
        {
            get
            {
                if (_context == null || _context.Database.Connection.State != System.Data.ConnectionState.Open)
                {
                    _context?.Dispose();
                    _context = new AppDbContext(_connectionString);
                }
                return _context;
            }
        }

        #region 数据库初始化 

        /// <summary>
        /// 初始化数据库，如果数据库不存在则创建
        /// </summary>
        private void InitializeDatabase()
        {
            try
            {
                if (!System.IO.File.Exists(_databasePath))
                {
                    SQLiteConnection.CreateFile(_databasePath);
                }

                using (var context = new AppDbContext(_connectionString))
                {
                    context.Database.CreateIfNotExists();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"数据库初始化失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 执行数据库迁移
        /// </summary>
        public void MigrateDatabase()
        {
            try
            {
                Context.Database.CreateIfNotExists();
            }
            catch (Exception ex)
            {
                throw new Exception($"数据库迁移失败: {ex.Message}", ex);
            }
        }

        #endregion

        #region 通用增删改查操作
        /// <summary>
        /// 添加实体
        /// </summary>
        public T Add<T>(T entity) where T : class
        {
            try
            {
                var result = Context.Set<T>().Add(entity);
                Context.SaveChanges();
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"添加实体失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 异步添加实体
        /// </summary>
        public async Task<T> AddAsync<T>(T entity) where T : class
        {
            try
            {
                var result = Context.Set<T>().Add(entity);
                await Context.SaveChangesAsync();
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"异步添加实体失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 批量添加实体
        /// </summary>
        public void AddRange<T>(IEnumerable<T> entities) where T : class
        {
            try
            {
                Context.Set<T>().AddRange(entities);
                Context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception($"批量添加实体失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 更新实体
        /// </summary>
        public void Update<T>(T entity) where T : class
        {
            try
            {
                Context.Entry(entity).State = EntityState.Modified;
                Context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception($"更新实体失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 异步更新实体
        /// </summary>
        public async Task UpdateAsync<T>(T entity) where T : class
        {
            try
            {
                Context.Entry(entity).State = EntityState.Modified;
                await Context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"异步更新实体失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 删除实体
        /// </summary>
        public void Delete<T>(T entity) where T : class
        {
            try
            {
                Context.Set<T>().Remove(entity);
                Context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception($"删除实体失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 根据ID删除实体
        /// </summary>
        public void DeleteById<T>(object id) where T : class
        {
            try
            {
                var entity = Context.Set<T>().Find(id);
                if (entity != null)
                {
                    Context.Set<T>().Remove(entity);
                    Context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"根据ID删除实体失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 根据条件删除实体
        /// </summary>
        public void DeleteWhere<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            try
            {
                var entities = Context.Set<T>().Where(predicate).ToList();
                Context.Set<T>().RemoveRange(entities);
                Context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception($"根据条件删除实体失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 获取所有实体
        /// </summary>
        public List<T> GetAll<T>() where T : class
        {
            try
            {
                return Context.Set<T>().ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"获取所有实体失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 根据ID获取实体
        /// </summary>
        public T GetById<T>(object id) where T : class
        {
            try
            {
                return Context.Set<T>().Find(id);
            }
            catch (Exception ex)
            {
                throw new Exception($"根据ID获取实体失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 根据条件获取实体
        /// </summary>
        public T GetFirstOrDefault<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            try
            {
                return Context.Set<T>().FirstOrDefault(predicate);
            }
            catch (Exception ex)
            {
                throw new Exception($"根据条件获取实体失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 根据条件获取实体列表
        /// </summary>
        public List<T> GetWhere<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            try
            {
                return Context.Set<T>().Where(predicate).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"根据条件获取实体列表失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        public List<T> GetPaged<T>(int pageIndex, int pageSize, Expression<Func<T, bool>> predicate = null) where T : class
        {
            try
            {
                IQueryable<T> query = Context.Set<T>();

                if (predicate != null)
                {
                    query = query.Where(predicate);
                }

                return query.Skip(pageIndex * pageSize).Take(pageSize).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"分页查询失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 获取记录数量
        /// </summary>
        public int Count<T>(Expression<Func<T, bool>> predicate = null) where T : class
        {
            try
            {
                if (predicate != null)
                {
                    return Context.Set<T>().Count(predicate);
                }
                return Context.Set<T>().Count();
            }
            catch (Exception ex)
            {
                throw new Exception($"获取记录数量失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 检查实体是否存在
        /// </summary>
        public bool Exists<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            try
            {
                return Context.Set<T>().Any(predicate);
            }
            catch (Exception ex)
            {
                throw new Exception($"检查实体是否存在失败: {ex.Message}", ex);
            }
        }
        #endregion

        #region 事务操作

        /// <summary>
        /// 执行事务操作
        /// </summary>
        public void ExecuteTransaction(Action<AppDbContext> action)
        {
            using (var transaction = Context.Database.BeginTransaction())
            {
                try
                {
                    action(Context);
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new Exception($"事务执行失败: {ex.Message}", ex);
                }
            }
        }

        /// <summary>
        /// 异步执行事务操作
        /// </summary>
        public async Task ExecuteTransactionAsync(Func<AppDbContext, Task> action)
        {
            using (var transaction = Context.Database.BeginTransaction())
            {
                try
                {
                    await action(Context);
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new Exception($"异步事务执行失败: {ex.Message}", ex);
                }
            }
        }

        #endregion

        #region 原生SQL操作

        /// <summary>
        /// 执行原生SQL查询
        /// </summary>
        public List<T> ExecuteRawSql<T>(string sql, params object[] parameters) where T : class
        {
            try
            {
                return Context.Database.SqlQuery<T>(sql, parameters).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"执行原生SQL查询失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 执行原生SQL命令
        /// </summary>
        public int ExecuteRawSqlCommand(string sql, params object[] parameters)
        {
            try
            {
                return Context.Database.ExecuteSqlCommand(sql, parameters);
            }
            catch (Exception ex)
            {
                throw new Exception($"执行原生SQL命令失败: {ex.Message}", ex);
            }
        }

        #endregion

        #region 工具方法

        /// <summary>
        /// 测试数据库连接
        /// </summary>
        public bool TestConnection()
        {
            try
            {
                Context.Database.Connection.Open();
                Context.Database.Connection.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 获取数据库大小（字节）
        /// </summary>
        public long GetDatabaseSize()
        {
            try
            {
                var fileInfo = new System.IO.FileInfo(_databasePath);
                return fileInfo.Length;
            }
            catch (Exception ex)
            {
                throw new Exception($"获取数据库大小失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 清空表数据
        /// </summary>
        public void ClearTable<T>() where T : class
        {
            try
            {
                var entities = Context.Set<T>().ToList();
                Context.Set<T>().RemoveRange(entities);
                Context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception($"清空表数据失败: {ex.Message}", ex);
            }
        }

        #endregion

        #region 资源释放

        public void Dispose()
        {
            _context?.Dispose();
        }

        #endregion
    }

    // 使用示例类
    public class DatabaseExample
    {
        private readonly SqliteDatabaseHelper _dbHelper;

        public DatabaseExample(string databasePath)
        {
            _dbHelper = new SqliteDatabaseHelper(databasePath);
        }

        public void ExampleUsage()
        {
            // 添加用户
            var family = new Family
            {
                FamilyName = "Xue",
                FamilyName_CN="薛"
            };
            _dbHelper.Add(family);
        }
    }
}
