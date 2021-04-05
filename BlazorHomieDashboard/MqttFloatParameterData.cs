using System;
using System.Threading.Tasks;

namespace BlazorHomieDashboard {
    public class MqttFloatParameterData {
        public string Caption { get; set; }
        public double ActualValue { get; set; }
        public string Units { get; set; }
        public double TargetValue { get; set; }
        public Action SetTargetValue { get; set; }
    }
}