using System.Collections.Generic;

namespace Bhd.Shared {
    public class Property {
        public string Id { get; set; }
        public string Path { get; set; }
        public string Name { get; set; }
        public string Bybis { get; set; }
        public PropertyType Type { get; set; }
        public float Value { get; set; }
        public List<string> Choices { get; set; }
    }
}