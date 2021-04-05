using System;
using System.Collections.Generic;

namespace BlazorHomieDashboard {
    public class MqttObject {
        public string Name { get; set; }
        public string DataType { get; set; }
        public string Format { get; set; }
        public bool Settable { get; set; }
        public bool Retained { get; set; }
        public string Unit { get; set; }
        public List<string> Properties = new ();
        public string NodeName { get; set; }
        public List<string> Nodes = new();
        public string Homie { get; set; }
        public string State { get; set; }
        public string Type { get; set; }
        public List<MqttObject> Children = new();
        public string Value { get; set; }
        public string Topic { get; set; }

        public Action ValueChanged = () => {};
    }
}
