using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace BlazorHomieDashboard.Server.Controllers {
    [ApiController]
    [Route("[controller]")]
    public class VersionController : ControllerBase {

        [HttpGet("SourceCodeUrl")]
        public IActionResult GetSourceCodeLink() {
            var versionInfo = (typeof(Program).GetTypeInfo().Assembly.GetCustomAttribute(typeof(AssemblyInformationalVersionAttribute)) as AssemblyInformationalVersionAttribute)?.InformationalVersion;

            if (versionInfo != null && versionInfo.Contains("-")) {
                var commitSha = versionInfo.Split("-")[1];
                return new JsonResult($"https://github.com/Girdauskas/BlazorHomieDashboard/tree/{commitSha}");
            } else {
                return new JsonResult($"http://github.com/Girdauskas/BlazorHomieDashboard/tree/{versionInfo}");
            }
        }

        [HttpGet]
        public IActionResult Get() {
            var versionInfo = (typeof(Program).GetTypeInfo().Assembly.GetCustomAttribute(typeof(AssemblyInformationalVersionAttribute)) as AssemblyInformationalVersionAttribute)?.InformationalVersion;
            return new JsonResult(versionInfo);
        }
    }
}