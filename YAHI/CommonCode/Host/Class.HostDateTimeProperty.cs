using System;

namespace DevBot9.Protocols.Homie {
    /// <summary>
    /// A property of type DateTime, as defined by the Homie convention.
    /// </summary>
    public class HostDateTimeProperty : HostPropertyBase {
        /// <summary>
        /// Set value will be published to the MQTT broker. Getting the property will retrieve value from the cache.
        /// </summary>
        public DateTime Value {
            get {
                var returnValue = Helpers.ParseDateTime(_rawValue);

                return returnValue;
            }
            set {
                SetValue(value);
            }
        }


        private readonly string _isoFormatString = "yyyy-MM-ddTHH:mm:ss.fff";

        internal HostDateTimeProperty(PropertyType propertyType, string propertyId, string friendlyName, DateTime initialValue) : base(propertyType, propertyId, friendlyName, DataType.DateTime, "", "") {
            _rawValue = initialValue.ToString(_isoFormatString);
        }

        internal override void Initialize(Device parentDevice) {
            base.Initialize(parentDevice);
        }

        protected override bool ValidatePayload(string payloadToValidate) {
            var isPayloadGood = Helpers.TryParseDateTime(payloadToValidate, out var _);

            return isPayloadGood;
        }

        private void SetValue(DateTime valueToSet) {
            switch (Type) {
                case PropertyType.State:
                case PropertyType.Parameter:
                    _rawValue = valueToSet.ToString(_isoFormatString);
                    _parentDevice.InternalPropertyPublish($"{_propertyId}", _rawValue);

                    break;

                case PropertyType.Command:
                    throw new InvalidOperationException();
            }
        }
    }
}
