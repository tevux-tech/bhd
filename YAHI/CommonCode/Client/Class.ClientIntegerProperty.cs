using System;

namespace DevBot9.Protocols.Homie {
    /// <summary>
    /// A property of type Integer, as defined by the Homie convention.
    /// </summary>
    public class ClientIntegerProperty : ClientPropertyBase {
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

        internal ClientIntegerProperty(ClientPropertyMetadata creationOptions) : base(creationOptions) {
            if (Helpers.TryParseInt(_rawValue, out var _) == false) { _rawValue = "0"; }
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
                case PropertyType.Parameter:
                case PropertyType.Command:
                    _rawValue = valueToSet.ToHomiePayload();
                    _parentDevice.InternalPropertyPublish($"{_propertyId}/set", _rawValue);
                    break;

                case PropertyType.State:
                    throw new InvalidOperationException();
            }
        }
    }
}
