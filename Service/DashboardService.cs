using FraudDetection.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;

namespace FraudDetection.Service
{
    public class DashboardService
    {
        public async Task<DashBoardDto> GetDashBoardData(string? userId)
        {
           using var connection = await CreateOpenConnection();

            using var data = await ExecuteDashboardQuery(connection, userId);
            if (await data.ReadAsync())
            {
                return new DashBoardDto
                {
                    total_transactions = Convert.ToInt32(data["total_transactions"]),
                    flagged_transactions = Convert.ToInt32(data["flagged_transactions"]),
                    high_risk = Convert.ToInt32(data["high_risk"]),
                    suspicious = Convert.ToInt32(data["suspicious"]) ,
                };
            }
            return new DashBoardDto();
        }
        private async Task<SqliteDataReader> ExecuteDashboardQuery(SqliteConnection connection,string? userId)
        {
            var command = connection.CreateCommand();

            if (!string.IsNullOrWhiteSpace(userId))
            {
                command.CommandText = @"SELECT COUNT(*) AS total_transactions, COALESCE(SUM(high_risk), 0) AS high_risk,COALESCE(SUM(suspicious), 0) AS suspicious, COALESCE(SUM(high_risk) + SUM(suspicious), 0) AS flagged_transactions FROM transactions WHERE user_id = $userId;";

                command.Parameters.AddWithValue("$userId", userId);
            }
            else
            {
                command.CommandText = @"SELECT COUNT(*) AS total_transactions, COALESCE(SUM(high_risk), 0) AS high_risk,COALESCE(SUM(suspicious), 0) AS suspicious, COALESCE(SUM(high_risk) + SUM(suspicious), 0) AS flagged_transactions FROM transactions;";
            }

            return await command.ExecuteReaderAsync();
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
