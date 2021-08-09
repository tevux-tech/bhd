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
        private readonly HomieService _homieService;


        private List<DashboardConfig> _configs = new();

        public DashboardsController(ILogger<DashboardsController> logger, HomieService homieService) {
            _logger = logger;
            _homieService = homieService;


            var demo1 = new DashboardConfig();
            demo1.DashboardName = "Demo dashboard 1";
            demo1.DashboardId = "demo1";

            var demoNode1 = new NodeConfig();
            demoNode1.NodeId = "node1";
            demoNode1.NodeName = "Demo node 1";
            demo1.Nodes.Add(demoNode1);

            var demoProperty = new PropertyConfig();
            demoProperty.PropertyName = "Custom name test";
            demoProperty.PropertyPath = "/api/devices/shelly1pm-68c63afadff9/nodes/basic/properties/actual-relay-state";
            demoNode1.Properties.Add(demoProperty);

            var demoNode2 = new NodeConfig();
            demoNode2.NodeId = "node2";
            demoNode2.NodeName = "Demo node 2";
            demo1.Nodes.Add(demoNode2);

            _configs.Add(demo1);
        }

        [HttpGet]
        public IEnumerable<Dashboard> Get() {
            var dashboards = new List<Dashboard>();

            foreach (var dashboardConfig in _configs) {
                var dashboard = new Dashboard();
                dashboard.Id = dashboardConfig.DashboardId;
                dashboard.Name = dashboardConfig.DashboardName;
                dashboard.Nodes = $"/api/dashboards/{dashboard.Id}/nodes";
               dashboards.Add(dashboard);
            }

            return dashboards;
        }

        [HttpGet("{dashboardId}")]
        public Dashboard GetDashboard(string dashboardId) {
            return Get().First(d => d.Id == dashboardId);
        }

        [HttpGet("{dashboardId}/Nodes")]
        public IEnumerable<DashboardNode> GetNodes(string dashboardId) {
            var nodes = new List<DashboardNode>();

            var dashboardConfig = _configs.First(d => d.DashboardId == dashboardId);

            foreach (var nodeConfig in dashboardConfig.Nodes) {
                var node = new DashboardNode();
                node.Name = nodeConfig.NodeName;
                node.Id = nodeConfig.NodeId;
                nodes.Add(node);

                foreach (var propertyConfig in nodeConfig.Properties) {
                    node.Properties[propertyConfig.PropertyName] = propertyConfig.PropertyPath;
                }
            }

            //var node1 = new Node();
            //node1.NodeId = "node1";
            //node1.Name = "Node 1";
            //node1.Properties = new List<Property>();
            //var p1 = PropertyFactory.Create(_homieService.DynamicConsumers[0].ClientDevice.Nodes[0].Properties[0], "shelly1pm-68c63afadff9", "basic");
            //node1.Properties.Add(p1);
            //node1.Properties.Add(p1);
            //node1.Properties.Add(p1);
            //node1.Properties.Add(p1);
            //node1.Properties.Add(p1);

            //nodes.Add(node1);

            //var node2 = new Node();
            //node2.NodeId = "node2";
            //node2.Name = "Node 2";
            //node2.Properties = new List<Property>();
            //nodes.Add(node2);

            return nodes;
        }
    }
}