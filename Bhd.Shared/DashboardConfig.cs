using System.Collections.Generic;

namespace Bhd.Shared {
    public class DashboardConfig {
        public string DashboardName { get; set; }
        public string DashboardId { get; set; }
        public List<NodeConfig> Nodes { get; set; } = new();
    }

    public class NodeConfig {
        public string NodeName { get; set; }
        public string NodeId { get; set; }

        public List<PropertyConfig> Properties { get; set; } = new();
    }

    public class PropertyConfig {
        public string PropertyName { get; set; }
        public string PropertyPath { get; set; }
    }
}
