using System.Threading.Tasks;
using BlazorHomieDashboard.Server.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BlazorHomieDashboard.Server.Controllers {
    [ApiController]
    [Route("[controller]")]
    public class DeviceController : ControllerBase {
        private readonly IHomieMqttService _homieMqttService;
        private readonly ILogger<DeviceController> _logger;

        public DeviceController(IHomieMqttService homieMqttService, ILogger<DeviceController> logger) {
            _homieMqttService = homieMqttService;
            _logger = logger;
        }

        [HttpPost("Remove")]
        public async Task<IActionResult> Remove([FromBody] string deviceId) {
            _logger.LogInformation("Removing " + deviceId);
            await _homieMqttService.RemoveDeviceTopics(deviceId);
            return Ok();
        }
    }
}