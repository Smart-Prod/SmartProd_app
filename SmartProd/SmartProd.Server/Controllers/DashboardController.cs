using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartProd.API.Server.Services;

namespace SmartProd.API.Server.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly DashboardService _dashboardService;

        public DashboardController(DashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet]
        public async Task<IActionResult> GetDashboard(CancellationToken cancellationToken)
        {
            var data = await _dashboardService.GetDashboardAsync(cancellationToken);
            return Ok(data);
        }
    }
}
