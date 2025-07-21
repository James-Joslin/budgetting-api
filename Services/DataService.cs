using System.Data;
using financesApi.models;
using financesApi.utilities;
using Microsoft.AspNetCore.Identity;
using Npgsql;

namespace financesApi.services
{
    // Generic Data Service - replaces your DataService
    public static class GenericDataService
    {
        // Generic read operation
        public static async Task<DataTable> ExecuteQueryAsync(string queryPath, TransactionQueryRequest? queryParamters)
        {
            string query = await MinioConnection.GetQueryAsync(queryPath)
                ?? throw new ArgumentNullException(nameof(query), $"Query '{queryPath}' returned null");

            if (queryParamters != null)
            {
                string filter = FilterBuilder.BuildFilter(queryParamters);

                query += filter;
            }

            return await PostgreSqlQuerier.ExecuteQueryAsync(query);
        }

        // Generic write operation
        public static async Task<int> ExecuteCommandAsync(string queryPath, Dictionary<string, object> parameters)
        {
            string query = await MinioConnection.GetQueryAsync(queryPath)
                ?? throw new ArgumentNullException(nameof(query), $"Query '{queryPath}' returned null");

            return await PostgreSqlQuerier.ExecuteNonQueryAsync(query, parameters);
        }

        // Insert and return ID
        public static async Task<T?> InsertAndReturnAsync<T>(string queryPath, Dictionary<string, object> parameters)
        {
            string query = await MinioConnection.GetQueryAsync(queryPath)
                ?? throw new ArgumentNullException(nameof(query), $"Query '{queryPath}' returned null");

            return await PostgreSqlQuerier.ExecuteScalarAsync<T>(query, parameters);
        }

        // Batch operations with transaction
        public static async Task ExecuteBatchAsync(List<(string queryPath, Dictionary<string, object> parameters)> operations)
        {
            await PostgreSqlQuerier.ExecuteTransactionAsync(async (connection, transaction) =>
            {
                foreach (var (queryPath, parameters) in operations)
                {
                    string query = await MinioConnection.GetQueryAsync(queryPath);
                    
                    using var command = new NpgsqlCommand(query, connection, transaction);
                    foreach (var param in parameters)
                    {
                        command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                    }
                    
                    await command.ExecuteNonQueryAsync();
                }
            });
        }
        // // NEW: Bulk insert method specifically for transactions
        // public static async Task<int> BulkInsertTransactionsAsync(List<OfxTransactionDto> transactions, int accountId, string? sourceFileName = null)
        // {
        //     if (!transactions.Any()) return 0;

        //     if (accountId <= 0)
        //         throw new ArgumentException("Valid accountId is required", nameof(accountId));

        //     using var connection = PostgreSqlQuerier.BuildConnection();
        //     await connection.OpenAsync();

        //     using var transaction = await connection.BeginTransactionAsync();
        //     try
        //     {
        //         // Use PostgreSQL COPY command for maximum performance
        //         var copyCommand = @"
        //             COPY transactions (account_id, date, amount, payee, memo, category, source_file, created_at) 
        //             FROM STDIN (FORMAT BINARY)";

        //         using var writer = await connection.BeginBinaryImportAsync(copyCommand, transaction);
                
        //         foreach (var tx in transactions)
        //         {
        //             await writer.StartRowAsync();
        //             await writer.WriteAsync(accountId, NpgsqlDbType.Integer);
        //             await writer.WriteAsync(tx.Date, NpgsqlDbType.Date);
        //             await writer.WriteAsync(tx.Amount, NpgsqlDbType.Numeric);
        //             await writer.WriteAsync(tx.Payee ?? "", NpgsqlDbType.Text);
        //             await writer.WriteAsync(tx.Memo ?? "", NpgsqlDbType.Text);
        //             await writer.WriteAsync((object?)null, NpgsqlDbType.Text); // category - null for now
        //             await writer.WriteAsync(sourceFileName ?? "", NpgsqlDbType.Text);
        //             await writer.WriteAsync(DateTime.Now, NpgsqlDbType.Timestamp); // timestamp without time zone
        //         }

        //         await writer.CompleteAsync();
        //         await transaction.CommitAsync();
                
        //         return transactions.Count;
        //     }
        //     catch
        //     {
        //         await transaction.RollbackAsync();
        //         throw;
        //     }
        // }

        //         // NEW: Check for duplicate transactions before inserting
        // public static async Task<List<OfxTransactionDto>> FilterDuplicateTransactionsAsync(
        //     List<OfxTransactionDto> transactions, 
        //     int accountId, 
        //     int daysTolerance = 1)
        // {
        //     if (!transactions.Any()) return transactions;

        //     var minDate = transactions.Min(t => t.Date).AddDays(-daysTolerance);
        //     var maxDate = transactions.Max(t => t.Date).AddDays(daysTolerance);

        //     var existingQuery = @"
        //         SELECT date, amount, payee, memo
        //         FROM transactions 
        //         WHERE account_id = @accountId 
        //           AND date BETWEEN @minDate AND @maxDate";

        //     var parameters = new Dictionary<string, object>
        //     {
        //         { "@accountId", accountId },
        //         { "@minDate", minDate },
        //         { "@maxDate", maxDate }
        //     };

        //     var existingTransactions = await PostgreSqlQuerier.ExecuteQueryAsync(existingQuery, parameters);
            
        //     var existingSet = new HashSet<string>();
        //     foreach (DataRow row in existingTransactions.Rows)
        //     {
        //         var key = $"{row["date"]}|{row["amount"]}|{row["payee"]}|{row["memo"]}";
        //         existingSet.Add(key);
        //     }

        //     return transactions.Where(tx =>
        //     {
        //         var key = $"{tx.Date:yyyy-MM-dd}|{tx.Amount}|{tx.Payee}|{tx.Memo}";
        //         return !existingSet.Contains(key);
        //     }).ToList();
        // }
    }
}