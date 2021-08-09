using System;

namespace Bhd.Shared {
    public class Device {
        public string Id { get; set; }
        public string Name { get; set; }
        public DeviceState State { get; set; }
        public string Nodes { get; set; }
    }
}