using System.Collections.Generic;

namespace Bhd.Shared {
    public class DashboardNode {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<DashboardProperty> Properties { get; set; } = new();
    }
}