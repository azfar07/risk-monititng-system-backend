using FraudDetection.Service;
using Microsoft.AspNetCore.Mvc;

namespace FraudDetection.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DashboardController : Controller
    {
        private readonly DashboardService _service;

        public DashboardController(DashboardService service)
        {
            _service = service;
        }
        [HttpGet]
        public async Task<IActionResult> DashboardStat([FromQuery] string? userId)
        {
            try
            {
                var result = await _service.GetDashBoardData(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }
    }
}
