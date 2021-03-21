﻿using System.Collections.Generic;

namespace BlazorHomiePlayground {
    public class MqttTabData {
        public string Caption { get; set; }
        public List<MqttTabData> SubTabs { get; set; } = new();
        public List<object> Controls { get; set; } = new();
    }
}