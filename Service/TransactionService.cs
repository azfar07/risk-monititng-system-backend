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

            long userTransactionCount;

            using (var countCommand = connection.CreateCommand())
            {
                countCommand.CommandText = "SELECT COUNT(*) FROM transactions WHERE user_id = $user;";
                countCommand.Parameters.AddWithValue("$user", transactionInfo.UserId);
                userTransactionCount = (long)await countCommand.ExecuteScalarAsync();
            }

            int highRiskAmount = transactionInfo.Amount > 20000? 1 :0;
            int SuspeciousAmount = userTransactionCount > 3 ? 1 :0;
            string ruleTriggered = highRiskAmount == 1 && SuspeciousAmount == 1 ? "R1,R2" : highRiskAmount == 1 ? "R1" : SuspeciousAmount == 1 ? "R2" : "";
            DateTime finalTimestamp = transactionInfo.Timestamp ?? DateTime.UtcNow;
            using var insertCommand = connection.CreateCommand();
            insertCommand.CommandText = @" INSERT INTO transactions (transaction_id, user_id, amount, timestamp, device_id, high_risk, suspicious, rule_triggered) VALUES ($transactionId, $userId, $amount, $time, $device, $high, $suspicious, $rule);";

            insertCommand.Parameters.AddWithValue("$transactionId", transactionInfo.TransactionId);
            insertCommand.Parameters.AddWithValue("$userId", transactionInfo.UserId);
            insertCommand.Parameters.AddWithValue("$amount", transactionInfo.Amount);
            insertCommand.Parameters.AddWithValue("$time", finalTimestamp);
            insertCommand.Parameters.AddWithValue("$device", transactionInfo.DeviceId);
            insertCommand.Parameters.AddWithValue("$high", highRiskAmount);
            insertCommand.Parameters.AddWithValue("$suspicious", SuspeciousAmount);
            insertCommand.Parameters.AddWithValue("$rule",ruleTriggered ?? (object)DBNull.Value);

            await insertCommand.ExecuteNonQueryAsync();
        }
        public async Task<List<TransactionDto>> GetTransactions(string? userId)
        {
            using var connection = await CreateOpenConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();

            if (!string.IsNullOrWhiteSpace(userId))
            {
                command.CommandText = @" SELECT transaction_id, user_id, amount, timestamp, device_id FROM transactions WHERE user_id = $userId ORDER BY timestamp DESC;";
                command.Parameters.AddWithValue("$userId", userId);
            }
            else
            {
                command.CommandText = @"SELECT transaction_id, user_id, amount, timestamp, device_id FROM transactions ORDER BY timestamp DESC;";
            }
            using var data = await command.ExecuteReaderAsync();
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
