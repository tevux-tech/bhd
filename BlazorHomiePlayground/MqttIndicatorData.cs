using System;

namespace BlazorHomiePlayground {
    public class MqttIndicatorData {
        public string Caption { get; set; }
        public string Value { get; set; }

        public event EventHandler StateChanged;

        public virtual void OnStateChanged() {
            Console.WriteLine("Notifying State changed");

            StateChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}