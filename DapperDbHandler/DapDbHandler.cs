using Dapper;
using Newtonsoft.Json;
using Serilog;
using System.Collections.Immutable;
using System.Data.SqlClient;
using System.Reflection;

namespace DapperDbHandler
{
    public class DapDbHandler
    {
        public string ConnectionString { get; }
        public ILogger Logger { get; }
        public DapDbHandler(string connectionString, ILogger logger)
        {
            ConnectionString = connectionString;
            Logger = logger;
        }
        public async Task<ImmutableList<T>> RetrieveDataAsync<T>(string sql, object? param = null) where T : class
        {
            Logger?.Debug($"RetrieveDataAsync sql: {sql}");
            try
            {
                // Create connection to SQL Server
                using SqlConnection connection = new(ConnectionString);
                // Open connection
                await connection.OpenAsync();
                var result = await connection.QueryAsync<T>(sql, param);
                var list = result.ToImmutableList();
                return list;
            }
            catch (Exception ex)
            {
                Logger?.Error(ex, $"RetrieveDataAsync sql: {sql}");
                throw;
            }
        }
        public async Task ExecuteSqlWithTx(ImmutableList<SqlContainer4Dapper> sqlCommands, bool doNotCommit = false)
        {
            int cnt = 0;
            using var connection = new SqlConnection(ConnectionString);
            await connection.OpenAsync();
            using var transaction = await connection.BeginTransactionAsync();
            try
            {
                foreach (var sqlCommand in sqlCommands)
                {
                    Logger?.Debug($"ExecuteSqlWithTx sql: {sqlCommand.Sql}");
                    Logger?.Debug($"ExecuteSqlWithTx Parameters: {JsonConvert.SerializeObject(sqlCommand.Parameters)}");
                    await connection.ExecuteAsync(sqlCommand.Sql, sqlCommand.Parameters, transaction);
                    if (++cnt % 50 == 0)
                    {
                        Logger?.Information($"Executed {cnt} commands.");
                    }
                }

                if (doNotCommit)
                {
                    await transaction.RollbackAsync();
                    return;
                }
                // Commit the transaction  
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                // Rollback the transaction if an error occurs  
                await transaction.RollbackAsync();
                Logger?.Error(ex, $"ExecuteSqlWithTx @{cnt}, sql: {sqlCommands[cnt].Sql}, Parameters: {JsonConvert.SerializeObject(sqlCommands[cnt].Parameters)}");
                throw;
            }
        }
        public async Task ExecuteSql(ImmutableList<SqlContainer4Dapper> sqlCommands)
        {
            using var connection = new SqlConnection(ConnectionString);
            await connection.OpenAsync();
            int cnt = 0;
            try
            {
                foreach (var sqlCommand in sqlCommands)
                {
                    await connection.ExecuteAsync(sqlCommand.Sql, sqlCommand.Parameters);
                    if (++cnt % 50 == 0)
                    {
                        Logger?.Information($"Executed {cnt} commands.");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger?.Error(ex, $"ExecuteSqlWithTx @{cnt}, sql: {sqlCommands[cnt].Sql}, Parameters: {JsonConvert.SerializeObject(sqlCommands[cnt].Parameters)}");
                throw;
            }
        }
        public async Task ExeNonDapSqlAsync(SqlContainer sqlContainer)
        {
            // Create connection to SQL Server
            using SqlConnection connection = new(ConnectionString);
            // Open connection
            await connection.OpenAsync();

            using SqlTransaction transaction = connection.BeginTransaction();
            try
            {
                await ExecuteNonQuerySqlAsync(connection, transaction, sqlContainer);

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Logger?.Error(ex, $"{this.GetType().Name} {MethodBase.GetCurrentMethod()?.Name!} {ex.Message}, sql: {JsonConvert.SerializeObject(sqlContainer)}");
                throw;
            }
        }
        private async Task ExecuteNonQuerySqlAsync(SqlConnection connection, SqlTransaction transaction, SqlContainer container)
        {
            Logger?.Debug($"ExecuteNonQuerySqlAsync: {container.Sql}");
            using SqlCommand command = new(container.Sql, connection, transaction);
            foreach (var pair in container.Parameters)
            {
                command.Parameters.AddWithValue(pair.Key, pair.Value ?? DBNull.Value);
            }
            var ret = await command.ExecuteNonQueryAsync();
            Logger?.Information($"ExecuteNonQuerySqlAsync: {ret} rows affected.");
        }
    }
}
