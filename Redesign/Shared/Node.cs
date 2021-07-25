using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bhd.Shared {
    public class Node {
        public string NodeId { get; set; }
        public string Name { get; set; }
        public List<Property> Properties { get; set; }
    }
}
