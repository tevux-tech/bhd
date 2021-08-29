using System.Collections.Generic;
using System.Reflection;
using Bhd.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Bhd.Server.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    public class VersionsController : ControllerBase {
        [HttpGet]
        public IEnumerable<Version> Get() {
            var versions = new List<Version>();

            var backendVersion = new Version();
            backendVersion.Component = "BHD Backend";
            backendVersion.VersionNumber = "v" + (typeof(Bhd.Server.Program).GetTypeInfo().Assembly.GetCustomAttribute(typeof(AssemblyInformationalVersionAttribute)) as AssemblyInformationalVersionAttribute)?.InformationalVersion;
            versions.Add(backendVersion);

            var frontendVersion = new Version();
            frontendVersion.Component = "BHD Frontend";
            frontendVersion.VersionNumber = "v" + (typeof(Bhd.Client.App).GetTypeInfo().Assembly.GetCustomAttribute(typeof(AssemblyInformationalVersionAttribute)) as AssemblyInformationalVersionAttribute)?.InformationalVersion;
            versions.Add(frontendVersion);

            var yahiVersion = new Version();
            yahiVersion.Component = "YAHI";
            yahiVersion.VersionNumber = "v" + (typeof(DevBot9.Protocols.Homie.Device).GetTypeInfo().Assembly.GetCustomAttribute(typeof(AssemblyInformationalVersionAttribute)) as AssemblyInformationalVersionAttribute)?.InformationalVersion;
            versions.Add(yahiVersion);

            return versions;
        }
    }
}
