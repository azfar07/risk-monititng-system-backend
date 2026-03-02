using FraudDetection.Dto;
using FraudDetection.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;

namespace FraudDetection.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TransactionController : Controller
    {
        private readonly TransactionService _service;

        public TransactionController(TransactionService service)
        {
            _service = service;   
        }

        [HttpPost]
        public async Task<IActionResult> PerformTransaction([FromBody] TransactionDto dto)
        {
            try
            {
                await _service.PerformTransaction(dto);
                return Ok(new { message = "Transaction stored successfully" });
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
            {
                return BadRequest("Duplicate transaction_id.");
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetTransactions([FromQuery] string? userId)
        {
            try
            {
                var result = await _service.GetTransactions(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }
    }
}
