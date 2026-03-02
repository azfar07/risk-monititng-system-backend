using FraudDetection.Dto;
using Microsoft.Data.Sqlite;
using System.Transactions;

namespace FraudDetection.Service
{
    public class TransactionService
    {
        public async Task PerformTransaction(TransactionDto transactionInfo)
        {
            using var connection = await CreateOpenConnection();
            long userTransactionCount = await GetUserTransactionCount(connection, transactionInfo.UserId);
            bool highRiskAmount = transactionInfo.Amount > 20000? true :false;
            bool suspeciousAmount = userTransactionCount > 3 ? true :false;
            DateTime finalTimestamp = transactionInfo.Timestamp ?? DateTime.UtcNow;
            await InsertTransaction(connection,transactionInfo,highRiskAmount,suspeciousAmount,finalTimestamp);
        }

        public async Task<List<TransactionDto>> GetTransactions(string? userId)
        {
            using var connection = await CreateOpenConnection();
            using var data = await ExecuteTransactionQuery(connection, userId);
            var result = new List <TransactionDto>();

            while(await data.ReadAsync()){
                result.Add(new TransactionDto
                {
                    TransactionId = data["transaction_id"].ToString(),
                    UserId = data["user_id"].ToString(),
                    Amount= Convert.ToDecimal(data["amount"]),
                    Timestamp = data["timestamp"]== DBNull.Value? null: Convert.ToDateTime(data["timestamp"]),
                    DeviceId = data["device_id"].ToString()
                });
            }
            return result;
        }
        private async Task<SqliteDataReader> ExecuteTransactionQuery(SqliteConnection connection,string? userId)
        {
            var command = connection.CreateCommand();

            if (!string.IsNullOrWhiteSpace(userId))
            {
                command.CommandText = @"SELECT transaction_id, user_id, amount, timestamp, device_id FROM transactions WHERE user_id = $userId ORDER BY timestamp DESC;";
                command.Parameters.AddWithValue("$userId", userId);
            }
            else
            {
                command.CommandText = @"SELECT transaction_id, user_id, amount, timestamp, device_id FROM transactions ORDER BY timestamp DESC;";
            }

            return await command.ExecuteReaderAsync();
        }

        private async Task InsertTransaction(SqliteConnection connection,TransactionDto transactionInfo,bool highRiskAmount, bool suspeciousAmount, DateTime finalTimestamp)
        {
            using var insertCommand = connection.CreateCommand();

            insertCommand.CommandText = @"INSERT INTO transactions (transaction_id, user_id, amount, timestamp, device_id, high_risk, suspicious) VALUES ($transactionId, $userId, $amount, $time, $device, $high, $suspicious);";

            insertCommand.Parameters.AddWithValue("$transactionId", transactionInfo.TransactionId);
            insertCommand.Parameters.AddWithValue("$userId", transactionInfo.UserId);
            insertCommand.Parameters.AddWithValue("$amount", transactionInfo.Amount);
            insertCommand.Parameters.AddWithValue("$time", finalTimestamp);
            insertCommand.Parameters.AddWithValue("$device", transactionInfo.DeviceId);
            insertCommand.Parameters.AddWithValue("$high", highRiskAmount);
            insertCommand.Parameters.AddWithValue("$suspicious", suspeciousAmount);

            await insertCommand.ExecuteNonQueryAsync();
        }
        private async Task<long> GetUserTransactionCount(SqliteConnection connection,string userId)
        {
            using var countCommand = connection.CreateCommand();

            countCommand.CommandText =
                "SELECT COUNT(*) FROM transactions WHERE user_id = $user;";

            countCommand.Parameters.AddWithValue("$user", userId);

            return (long)await countCommand.ExecuteScalarAsync();
        }
        private static async Task<SqliteConnection> CreateOpenConnection()
        {
            var dbPath = Path.Combine(Directory.GetCurrentDirectory(), "user.db");
            var connectionString = $"Data Source={dbPath}";

            var connection = new SqliteConnection(connectionString);
            await connection.OpenAsync();

            return connection;
        }
    }
}
