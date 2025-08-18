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
        public static async Task<DataTable> ExecuteQueryAsync(string queryPath, TransactionQueryRequest? queryParameters = null)
        {
            string query = await MinioConnection.GetQueryAsync(queryPath)
                ?? throw new ArgumentNullException(nameof(query), $"Query '{queryPath}' returned null");

            if (queryParameters != null)
            {
                string filter = FilterBuilder.BuildFilter(queryParameters);

                query += filter;
            }

            return await PostgreSqlQuerier.ExecuteQueryAsync(query);
        }

        public static async Task<DataTable> ExecuteParameterisedQueryAsync(string queryPath, Dictionary<string, object> parameters)
        {
            string query = await MinioConnection.GetQueryAsync(queryPath)
                ?? throw new ArgumentNullException(nameof(query), $"Query '{queryPath}' returned null");
            // Console.WriteLine(query);
            return await PostgreSqlQuerier.ExecuteParameterisedQueryAsync(query, parameters);
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

        public static async Task<List<OfxTransactionDto>> FilterAndInsertTransactionsAsync(
            List<OfxTransactionDto> incomingTransactions,
            int accountId,
            int daysTolerance = 1)
        {
            if (!incomingTransactions.Any()) return new List<OfxTransactionDto>();

            // Step 1: Get date range
            var minDate = incomingTransactions.Min(t => t.Date).AddDays(-daysTolerance);
            var maxDate = incomingTransactions.Max(t => t.Date).AddDays(daysTolerance);

            // Step 2: Query existing transactions in that range
            var existingQuery = @"
                SELECT transaction_date, amount, payee, memo, fitid, transaction_type
                FROM transactions 
                WHERE account_id = @accountId 
                AND transaction_date BETWEEN @minDate AND @maxDate";

            var parameters = new Dictionary<string, object>
            {
                { "@accountId", accountId },
                { "@minDate", minDate },
                { "@maxDate", maxDate }
            };

            var existingTransactions = await PostgreSqlQuerier.ExecuteParameterisedQueryAsync(existingQuery, parameters);

            // Step 3: Build hash set of existing transaction keys
            var existingKeys = new HashSet<string>();
            foreach (DataRow row in existingTransactions.Rows)
            {
                var key = $"{row["transaction_date"]:yyyy-MM-dd}|{row["amount"]}|{row["payee"]}|{row["memo"]}|{row["fitid"]}|{row["transaction_type"]}";
                existingKeys.Add(key);
            }

            // Step 4: Filter incoming transactions
            var newTransactions = incomingTransactions.Where(tx =>
            {
                var key = $"{tx.Date:yyyy-MM-dd}|{tx.Amount}|{tx.Payee}|{tx.Memo}|{tx.FitId}|{tx.transType}";
                return !existingKeys.Contains(key);
            }).ToList();

            // Step 5: Insert new transactions
            foreach (var tx in newTransactions)
            {
                var insertQuery = @"
                    INSERT INTO transactions (account_id, transaction_date, amount, payee, memo, fitid, transaction_type)
                    VALUES (@accountId, @transaction_date, @amount, @payee, @memo, @fitid, @transaction_type)";

                var insertParams = new Dictionary<string, object>
                {
                    { "@accountId", accountId },
                    { "@transaction_date", tx.Date },
                    { "@amount", tx.Amount },
                    { "@payee", tx.Payee },
                    { "@memo", tx.Memo },
                    { "@fitid", tx.FitId },
                    { "@transaction_type", tx.transType }
                };

                await PostgreSqlQuerier.ExecuteNonQueryAsync(insertQuery, insertParams);
            }

            return newTransactions;
        }
    }
}