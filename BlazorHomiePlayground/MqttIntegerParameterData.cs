using System;

namespace BlazorHomiePlayground {
    public class MqttIntegerParameterData {
        public string Caption { get; set; }
        public int ActualValue { get; set; }
        public string Units { get; set; }
        public int TargetValue { get; set; }
        public Action SetTargetValue { get; set; }
    }
}