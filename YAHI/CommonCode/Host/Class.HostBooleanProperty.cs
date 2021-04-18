using System;

namespace DevBot9.Protocols.Homie {
    /// <summary>
    /// A property of type Boolean, as defined by the Homie convention.
    /// </summary>
    public class HostBooleanProperty : HostPropertyBase {
        /// <summary>
        /// Set value will be published to the MQTT broker. Getting the property will retrieve value from the cache.
        /// </summary>
        public bool Value {
            get {
                bool returnValue;

                returnValue = Helpers.ParseBool(_rawValue);

                return returnValue;
            }
            set {
                SetValue(value);
            }
        }

        internal HostBooleanProperty(PropertyType propertyType, string propertyId, string friendlyName, bool initialValue, string format, string unit) : base(propertyType, propertyId, friendlyName, DataType.Boolean, format, unit) {
            _rawValue = initialValue.ToString().ToLower();
        }

        internal override void Initialize(Device parentDevice) {
            base.Initialize(parentDevice);
        }

        protected override bool ValidatePayload(string payloadToValidate) {
            var returnValue = false;

            if ((payloadToValidate == "true") || (payloadToValidate == "false")) {
                returnValue = true;
            }

            return returnValue;
        }

        private void SetValue(bool valueToSet) {
            switch (Type) {
                case PropertyType.State:
                case PropertyType.Parameter:
                    if (valueToSet == true) {
                        _rawValue = "true";
                    }
                    else {
                        _rawValue = "false";
                    }

                    _parentDevice.InternalPropertyPublish($"{_propertyId}", _rawValue);
                    break;

                case PropertyType.Command:
                    throw new InvalidOperationException();
            }
        }
    }
}
