using System;

namespace BlazorHomieDashboard {
    public class MqttCommandData {
        public string Caption { get; set; }
        public string Button1Caption { get; set; }
        public string Button2Caption { get; set; }

        public Action ExecuteButton1Action { get; set; }
        public Action ExecuteButton2Action { get; set; }
    }
}