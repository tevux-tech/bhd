using System.Collections.Generic;
using System.Linq;
using Bhd.Server.Services;
using Bhd.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Bhd.Server.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardsController : ControllerBase {
        private readonly ILogger<DashboardsController> _logger;
        private readonly UserDashboardsStorage _storage;

        public DashboardsController(ILogger<DashboardsController> logger, UserDashboardsStorage storage) {
            _logger = logger;
            _storage = storage;
        }

        [HttpGet]
        public IEnumerable<Dashboard> Get() {
            var dashboards = new List<Dashboard>();

            foreach (var dashboardConfig in _storage.Dashboards) {
                var dashboard = new Dashboard();
                dashboard.Id = dashboardConfig.DashboardId;
                dashboard.Name = dashboardConfig.DashboardName;
                dashboard.Nodes = $"/api/dashboards/{dashboard.Id}/nodes";
               dashboards.Add(dashboard);
            }

            return dashboards;
        }

        [HttpGet("configuration")]
        public List<DashboardConfig> GetConfiguration() {
            return _storage.Dashboards;
        }

        [HttpPut("configuration")]
        public void SetConfiguration([FromBody] List<DashboardConfig> newConfiguration) {
            _storage.UpdateDashboards(newConfiguration);
        }

        [HttpGet("{dashboardId}")]
        public Dashboard GetDashboard(string dashboardId) {
            return Get().First(d => d.Id == dashboardId);
        }

        [HttpGet("{dashboardId}/Nodes")]
        public IEnumerable<DashboardNode> GetNodes(string dashboardId) {
            var nodes = new List<DashboardNode>();

            var dashboardConfig = _storage.Dashboards.First(d => d.DashboardId == dashboardId);

            foreach (var nodeConfig in dashboardConfig.Nodes) {
                var node = new DashboardNode();
                node.Name = nodeConfig.NodeName;
                nodes.Add(node);

                foreach (var propertyConfig in nodeConfig.Properties) {
                    var dashboardProperty = new DashboardProperty();
                    dashboardProperty.AlternativeName = propertyConfig.PropertyName;
                    dashboardProperty.ActualPropertyPath = propertyConfig.PropertyPath;
                    node.Properties.Add(dashboardProperty);
                }
            }

            return nodes;
        }
    }
}