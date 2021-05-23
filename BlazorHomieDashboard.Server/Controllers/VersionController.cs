using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace BlazorHomieDashboard.Server.Controllers {
    [ApiController]
    [Route("[controller]")]
    public class VersionController : ControllerBase {
        [HttpGet]
        public IActionResult Get() {
            var versionInfo = (typeof(Program).GetTypeInfo().Assembly.GetCustomAttribute(typeof(AssemblyInformationalVersionAttribute)) as AssemblyInformationalVersionAttribute)?.InformationalVersion;
            return new JsonResult(versionInfo);
        }
    }
}