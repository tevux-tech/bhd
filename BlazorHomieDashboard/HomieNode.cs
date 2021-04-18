using System.Collections.Generic;
using DevBot9.Protocols.Homie;

namespace BlazorHomieDashboard {
    public class HomieNode {
        public string Name { get; set; }
        public List<ClientPropertyBase> Properties = new();
    }
}
