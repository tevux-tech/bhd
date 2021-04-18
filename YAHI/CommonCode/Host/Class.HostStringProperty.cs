using System;

namespace DevBot9.Protocols.Homie {
    /// <summary>
    /// A property of type String, as defined by the Homie convention.
    /// </summary>
    public class HostStringProperty : HostPropertyBase {
        /// <summary>
        /// Set value will be published to the MQTT broker. Getting the property will retrieve value from the cache.
        /// </summary>
        public string Value {
            get {
                return _rawValue;
            }
            set {
                SetValue(value);
            }
        }

        internal HostStringProperty(PropertyType propertyType, string propertyId, string friendlyName, string initialValue, string format, string unit) : base(propertyType, propertyId, friendlyName, DataType.String, format, unit) {
            _rawValue = initialValue;
        }

        internal override void Initialize(Device parentDevice) {
            base.Initialize(parentDevice);
        }
        protected override bool ValidatePayload(string payloadToValidate) {
            return true;
        }

        private void SetValue(string valueToSet) {
            switch (Type) {
                case PropertyType.State:
                case PropertyType.Parameter:
                    _rawValue = valueToSet;

                    _parentDevice.InternalPropertyPublish($"{_propertyId}", _rawValue);
                    break;

                case PropertyType.Command:
                    throw new InvalidOperationException();
            }
        }
    }
}
