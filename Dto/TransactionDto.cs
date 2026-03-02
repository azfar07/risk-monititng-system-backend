using System.ComponentModel.DataAnnotations;

namespace FraudDetection.Dto
{
    public class TransactionDto
    {
        [Required]
        public required string TransactionId { get; set; }
        [Required]
        public required string UserId { get; set; }
        [Required]
        public decimal Amount { get; set; }
        public DateTime? Timestamp { get; set; }
        [Required]
        public required string DeviceId { get; set; }
    }
}
