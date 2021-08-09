using System.Collections.Generic;

namespace Bhd.Shared {
    public class Node {
        public string NodeId { get; set; }
        public string Name { get; set; }
        public List<Property> Properties { get; set; } = new();
    }
}
