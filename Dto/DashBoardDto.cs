namespace FraudDetection.Dto
{
    public class DashBoardDto
    {
        public int total_transactions { get; set; } = 0;
        public int flagged_transactions { get; set; } = 0;
        public int high_risk { get; set; } = 0;
        public int suspicious { get; set; } = 0;
    }
}
