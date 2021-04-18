using System;

namespace DevBot9.Protocols.Homie {
    public class PropertyChangedEventArgs : EventArgs {
        public string PropertyName { get; private set; }
        public PropertyChangedEventArgs(string propertyName) {
            PropertyName = propertyName;
        }
    }
}
