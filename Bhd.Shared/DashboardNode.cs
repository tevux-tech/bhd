using System.Collections.Generic;

namespace Bhd.Shared {
    public class DashboardNode {
        public string Id { get; set; }
        public string Name { get; set; }
        public Dictionary<string, string> Properties { get; set; } = new();
    }
}