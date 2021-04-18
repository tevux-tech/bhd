using System;

namespace DevBot9.Protocols.Homie {
    /// <summary>
    /// A property of type Integer, as defined by the Homie convention.
    /// </summary>
    public class HostIntegerProperty : HostPropertyBase {
        /// <summary>
        /// Set value will be published to the MQTT broker. Getting the property will retrieve value from the cache.
        /// </summary>
        public int Value {
            get {
                int returnValue;

                returnValue = int.Parse(_rawValue);

                return returnValue;
            }
            set {
                SetValue(value);
            }
        }

        internal HostIntegerProperty(PropertyType propertyType, string propertyId, string friendlyName, int initialValue, string format, string unit) : base(propertyType, propertyId, friendlyName, DataType.Integer, format, unit) {
            _rawValue = initialValue.ToString();
        }

        internal override void Initialize(Device parentDevice) {
            base.Initialize(parentDevice);
        }

        protected override bool ValidatePayload(string payloadToValidate) {
            var returnValue = Helpers.TryParseInt(payloadToValidate, out _);

            return returnValue;
        }

        private void SetValue(int valueToSet) {
            switch (Type) {
                case PropertyType.State:
                case PropertyType.Parameter:

                    _rawValue = valueToSet.ToString();

                    _parentDevice.InternalPropertyPublish($"{_propertyId}", _rawValue);
                    break;

                case PropertyType.Command:
                    throw new InvalidOperationException();
            }
        }
    }
}
