using Dapper;
using System.Collections.Immutable;
using System.Data.SqlClient;

namespace DapperDbHandler
{
    public class DapperDbHandler
    {
        public string ConnectionString { get; }
        public DapperDbHandler(string connectionString)
        {
            ConnectionString = connectionString;
        }
        public async Task<ImmutableList<T>> RetrieveDataAsync<T>(string sql, object? param = null) where T : class
        {
            // Create connection to SQL Server
            using SqlConnection connection = new(ConnectionString);
            // Open connection
            await connection.OpenAsync();
            return (await connection.QueryAsync<T>(sql, param)).ToImmutableList();
        }
        public async Task ExecuteSqlWithTx(ImmutableList<SqlCommandContainer> sqlCommands)
        {
            using var connection = new SqlConnection(ConnectionString);
            await connection.OpenAsync();
            using var transaction = await connection.BeginTransactionAsync();
            try
            {
                // Execute SQL statements using Dapper  
                foreach (var sqlCommand in sqlCommands)
                {
                    await connection.ExecuteAsync(sqlCommand.Sql, sqlCommand.Parameters, transaction);
                }

                // Commit the transaction  
                await transaction.CommitAsync();
            }
            catch
            {
                // Rollback the transaction if an error occurs  
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
