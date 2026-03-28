using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Services.Maps;
using WpfPrismFrameworkTemplate.Model;

namespace WpfPrismFrameworkTemplate.Helper
{
    /// <summary>
    /// SQL Server 数据库操作助手类
    /// </summary>
    public class SqlServerDatabaseHelper : IDisposable
    {
        private string _connectionString;
        private SqlConnection _connection;
        private bool _disposed = false;

        public async Task<bool> TestDatabaseConnection(string connectionString, string databaseName)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    DebugHelper.Instance.Log($"✓ 成功连接到数据库: {databaseName}");

                    // 获取数据库版本信息
                    using (var command = new SqlCommand("SELECT @@VERSION", connection))
                    {
                        var version = await command.ExecuteScalarAsync();
                        DebugHelper.Instance.Log($"服务器版本: {version?.ToString()?.Split('\n')[0]}");
                    }

                    return true;
                }
            }
            catch (SqlException ex)
            {
                DebugHelper.Instance.Log($"✗ 数据库连接失败: {ex.Message}",MsgLevel.Alarm);
                return false;
            }
        }

        public async Task<bool> CreateDatabase(string masterConnectionString, string databaseName)
        {
            try
            {
                using (var connection = new SqlConnection(masterConnectionString))
                {
                    await connection.OpenAsync();
                    DebugHelper.Instance.Log("✓ 成功连接到master数据库");

                    // 检查数据库是否已存在
                    string checkSql = "SELECT COUNT(*) FROM sys.databases WHERE name = @DatabaseName";
                    using (var checkCommand = new SqlCommand(checkSql, connection))
                    {
                        checkCommand.Parameters.AddWithValue("@DatabaseName", databaseName);
                        int count = (int)await checkCommand.ExecuteScalarAsync();

                        if (count > 0)
                        {
                            DebugHelper.Instance.Log("✓ 数据库已存在",MsgLevel.Warning);
                            return true;
                        }
                    }

                    string createSql = "";
                    // 创建数据库
                    //string createSql = $@"
                    //    CREATE DATABASE [{databaseName}]
                    //    ON (
                    //        NAME = '{databaseName}',
                    //        FILENAME = '{GetDatabaseFilePath(databaseName)}.mdf',
                    //        SIZE = 10MB,
                    //        MAXSIZE = 100MB,
                    //        FILEGROWTH = 5MB
                    //    )
                    //    LOG ON (
                    //        NAME = '{databaseName}_Log',
                    //        FILENAME = '{GetDatabaseFilePath(databaseName)}.ldf',
                    //        SIZE = 5MB,
                    //        MAXSIZE = 50MB,
                    //        FILEGROWTH = 5MB
                    //    )";

                    using (var createCommand = new SqlCommand(createSql, connection))
                    {
                        await createCommand.ExecuteNonQueryAsync();
                        DebugHelper.Instance.Log($"✓ 数据库 [{databaseName}] 创建成功");
                        return true;
                    }
                }
            }
            catch (SqlException ex)
            {
                DebugHelper.Instance.Log($"✗ 创建数据库失败: {ex.Message}",MsgLevel.Alarm);
                return false;
            }
        }


        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="connectionString">数据库连接字符串</param>
        public SqlServerDatabaseHelper(string connectionString = "Server=.;Database=DefaultDB;Trusted_Connection=True;")
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public void SetConnectionString(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentNullException(nameof(connectionString));

            _connectionString = connectionString;
        }

        /// <summary>
        /// 获取数据库连接
        /// </summary>
        /// <returns>SqlConnection对象</returns>
        private SqlConnection GetConnection()
        {
            if (_connection == null || _connection.State == ConnectionState.Closed)
            {
                _connection = new SqlConnection(_connectionString);
                _connection.Open();
            }
            return _connection;
        }

        #region 查询操作

        /// <summary>
        /// 执行查询并返回DataTable
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="parameters">参数</param>
        /// <returns>DataTable</returns>
        public DataTable ExecuteQuery(string sql, params SqlParameter[] parameters)
        {
            using (var command = CreateCommand(sql, parameters))
            {
                using (var adapter = new SqlDataAdapter(command))
                {
                    var dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    return dataTable;
                }
            }
        }

        /// <summary>
        /// 异步执行查询并返回DataTable
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="parameters">参数</param>
        /// <returns>DataTable</returns>
        public async Task<DataTable> ExecuteQueryAsync(string sql, params SqlParameter[] parameters)
        {
            return await Task.Run(() => ExecuteQuery(sql, parameters));
        }

        /// <summary>
        /// 执行查询并返回首行首列的值
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="parameters">参数</param>
        /// <returns>查询结果</returns>
        public object ExecuteScalar(string sql, params SqlParameter[] parameters)
        {
            using (var command = CreateCommand(sql, parameters))
            {
                return command.ExecuteScalar();
            }
        }

        /// <summary>
        /// 异步执行查询并返回首行首列的值
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="parameters">参数</param>
        /// <returns>查询结果</returns>
        public async Task<object> ExecuteScalarAsync(string sql, params SqlParameter[] parameters)
        {
            using (var command = CreateCommand(sql, parameters))
            {
                return await command.ExecuteScalarAsync();
            }
        }

        /// <summary>
        /// 执行查询并返回DataReader
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="parameters">参数</param>
        /// <returns>SqlDataReader</returns>
        public SqlDataReader ExecuteReader(string sql, params SqlParameter[] parameters)
        {
            var command = CreateCommand(sql, parameters);
            return command.ExecuteReader();
        }

        /// <summary>
        /// 异步执行查询并返回DataReader
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="parameters">参数</param>
        /// <returns>SqlDataReader</returns>
        public async Task<SqlDataReader> ExecuteReaderAsync(string sql, params SqlParameter[] parameters)
        {
            var command = CreateCommand(sql, parameters);
            return await command.ExecuteReaderAsync();
        }

        #endregion

        #region 非查询操作

        /// <summary>
        /// 执行非查询操作（INSERT、UPDATE、DELETE）
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="parameters">参数</param>
        /// <returns>受影响的行数</returns>
        public int ExecuteNonQuery(string sql, params SqlParameter[] parameters)
        {
            using (var command = CreateCommand(sql, parameters))
            {
                return command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// 异步执行非查询操作
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="parameters">参数</param>
        /// <returns>受影响的行数</returns>
        public async Task<int> ExecuteNonQueryAsync(string sql, params SqlParameter[] parameters)
        {
            using (var command = CreateCommand(sql, parameters))
            {
                return await command.ExecuteNonQueryAsync();
            }
        }

        #endregion

        #region 事务操作

        /// <summary>
        /// 执行事务
        /// </summary>
        /// <param name="actions">事务操作集合</param>
        /// <returns>是否成功</returns>
        public bool ExecuteTransaction(List<(string sql, SqlParameter[] parameters)> actions)
        {
            var connection = GetConnection();
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    foreach (var action in actions)
                    {
                        using (var command = new SqlCommand(action.sql, connection, transaction))
                        {
                            if (action.parameters != null)
                            {
                                command.Parameters.AddRange(action.parameters);
                            }
                            command.ExecuteNonQuery();
                        }
                    }
                    transaction.Commit();
                    return true;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// 异步执行事务
        /// </summary>
        /// <param name="actions">事务操作集合</param>
        /// <returns>是否成功</returns>
        public async Task<bool> ExecuteTransactionAsync(List<(string sql, SqlParameter[] parameters)> actions)
        {
            var connection = GetConnection();
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    foreach (var action in actions)
                    {
                        using (var command = new SqlCommand(action.sql, connection, transaction))
                        {
                            if (action.parameters != null)
                            {
                                command.Parameters.AddRange(action.parameters);
                            }
                            await command.ExecuteNonQueryAsync();
                        }
                    }
                    transaction.Commit();
                    return true;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        #endregion

        #region 批量操作

        /// <summary>
        /// 批量插入数据
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="dataTable">数据表</param>
        public void BulkInsert(string tableName, DataTable dataTable)
        {
            using (var bulkCopy = new SqlBulkCopy(GetConnection()))
            {
                bulkCopy.DestinationTableName = tableName;
                bulkCopy.WriteToServer(dataTable);
            }
        }

        /// <summary>
        /// 异步批量插入数据
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="dataTable">数据表</param>
        public async Task BulkInsertAsync(string tableName, DataTable dataTable)
        {
            using (var bulkCopy = new SqlBulkCopy(GetConnection()))
            {
                bulkCopy.DestinationTableName = tableName;
                await bulkCopy.WriteToServerAsync(dataTable);
            }
        }

        #endregion

        #region 存储过程操作

        /// <summary>
        /// 执行存储过程
        /// </summary>
        /// <param name="procedureName">存储过程名称</param>
        /// <param name="parameters">参数</param>
        /// <returns>DataTable</returns>
        public DataTable ExecuteStoredProcedure(string procedureName, params SqlParameter[] parameters)
        {
            using (var command = new SqlCommand(procedureName, GetConnection()))
            {
                command.CommandType = CommandType.StoredProcedure;
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }

                using (var adapter = new SqlDataAdapter(command))
                {
                    var dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    return dataTable;
                }
            }
        }

        /// <summary>
        /// 执行存储过程（非查询）
        /// </summary>
        /// <param name="procedureName">存储过程名称</param>
        /// <param name="parameters">参数</param>
        /// <returns>受影响的行数</returns>
        public int ExecuteStoredProcedureNonQuery(string procedureName, params SqlParameter[] parameters)
        {
            using (var command = new SqlCommand(procedureName, GetConnection()))
            {
                command.CommandType = CommandType.StoredProcedure;
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }
                return command.ExecuteNonQuery();
            }
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 创建SqlCommand对象
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="parameters">参数</param>
        /// <returns>SqlCommand</returns>
        private SqlCommand CreateCommand(string sql, params SqlParameter[] parameters)
        {
            var command = new SqlCommand(sql, GetConnection());
            if (parameters != null)
            {
                command.Parameters.AddRange(parameters);
            }
            return command;
        }

        /// <summary>
        /// 创建参数
        /// </summary>
        /// <param name="parameterName">参数名</param>
        /// <param name="value">参数值</param>
        /// <returns>SqlParameter</returns>
        public static SqlParameter CreateParameter(string parameterName, object value)
        {
            return new SqlParameter(parameterName, value ?? DBNull.Value);
        }

        /// <summary>
        /// 测试数据库连接
        /// </summary>
        /// <returns>是否连接成功</returns>
        public bool TestConnection()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    return connection.State == ConnectionState.Open;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 异步测试数据库连接
        /// </summary>
        /// <returns>是否连接成功</returns>
        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    return connection.State == ConnectionState.Open;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 获取数据库版本信息
        /// </summary>
        /// <returns>版本信息</returns>
        public string GetDatabaseVersion()
        {
            return ExecuteScalar("SELECT @@VERSION")?.ToString();
        }

        /// <summary>
        /// 检查表是否存在
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <returns>是否存在</returns>
        public bool TableExists(string tableName)
        {
            var sql = @"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES 
                       WHERE TABLE_NAME = @TableName";
            var result = ExecuteScalar(sql, CreateParameter("@TableName", tableName));
            return Convert.ToInt32(result) > 0;
        }

        #endregion

        #region IDisposable 实现

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="disposing">是否正在释放</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _connection?.Close();
                    _connection?.Dispose();
                }
                _disposed = true;
            }
        }

        /// <summary>
        /// 析构函数
        /// </summary>
        ~SqlServerDatabaseHelper()
        {
            Dispose(false);
        }

        #endregion
    }
}
