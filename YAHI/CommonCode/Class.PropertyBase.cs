namespace DevBot9.Protocols.Homie {
    /// <summary>
    /// Base class for all Client and Host properties. Should not be consumed directly.
    /// </summary>
    public class PropertyBase : INotifyPropertyChanged {
        /// <summary>
        /// Event is raised when the property is changed externally, that is, when an update is received from the MQTT broker.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged = delegate { };


        protected string _propertyId;
        protected Device _parentDevice;

        internal PropertyBase() {
            // Just making public constructor unavailable to user, as this class should not be consumed directly.
        }

        internal virtual void Initialize(Device parentDevice) { }

        internal void RaisePropertyChanged(object sender, PropertyChangedEventArgs e) {
            PropertyChanged(sender, e);
        }
    }
}
