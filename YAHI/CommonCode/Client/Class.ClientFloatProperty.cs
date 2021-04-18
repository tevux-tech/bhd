using System;

namespace DevBot9.Protocols.Homie {
    /// <summary>
    /// A property of type Float, as defined by the Homie convention.
    /// </summary>
    public class ClientFloatProperty : ClientPropertyBase {
        /// <summary>
        /// Set value will be published to the MQTT broker. Getting the property will retrieve value from the cache.
        /// </summary>
        public float Value {
            get {
                float returnValue;
                returnValue = Helpers.ParseFloat(_rawValue);

                return returnValue;
            }
            set {
                SetValue(value);
            }
        }

        internal ClientFloatProperty(ClientPropertyMetadata creationOptions) : base(creationOptions) {
            if (Helpers.TryParseFloat(_rawValue, out var _) == false) { _rawValue = "0.0"; }
        }

        internal override void Initialize(Device parentDevice) {
            base.Initialize(parentDevice);
        }

        protected override bool ValidatePayload(string payloadToValidate) {
            var returnValue = Helpers.TryParseFloat(payloadToValidate, out _);

            return returnValue;
        }

        private void SetValue(float valueToSet) {
            switch (Type) {
                case PropertyType.Parameter:
                case PropertyType.Command:
                    _rawValue = Helpers.FloatToString(valueToSet);
                    _parentDevice.InternalPropertyPublish($"{_propertyId}/set", _rawValue);
                    break;

                case PropertyType.State:
                    throw new InvalidOperationException();
            }
        }
    }
}
