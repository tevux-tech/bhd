using System;

namespace DevBot9.Protocols.Homie {
    /// <summary>
    /// A property of type Enum, as defined by the Homie convention.
    /// </summary>
    public class HostEnumProperty : HostPropertyBase {
        /// <summary>
        /// Setting this property will invoke validator and if it passes then the value will be published to the MQTT broker. Getting the property will retrieve value from the cache.
        /// </summary>
        public string Value {
            get {
                var returnValue = _rawValue;

                return returnValue;
            }
            set {
                SetValue(value);
            }
        }

        internal HostEnumProperty(PropertyType propertyType, string propertyId, string friendlyName, in string[] possibleValues, string initialValue) : base(propertyType, propertyId, friendlyName, DataType.Enum, "option1,option2", "") {
            if (possibleValues.Length == 0) { throw new ArgumentException("Please provide at least one correct value for this property", nameof(possibleValues)); }
            if (string.IsNullOrEmpty(initialValue) == false) {
                var isMatchFound = false;
                foreach (var value in possibleValues) {
                    if (value == initialValue) { isMatchFound = true; }
                }

                if (isMatchFound == false) { throw new ArgumentException("Initial value is not one of the possible values", nameof(initialValue)); }
            }

            var localFormat = possibleValues[0];
            for (var i = 1; i < possibleValues.Length; i++) {
                localFormat += "," + possibleValues[i];
            }
            _formatAttribute = localFormat;
            _rawValue = possibleValues[0];
        }

        internal override void Initialize(Device parentDevice) {
            base.Initialize(parentDevice);
        }

        protected override bool ValidatePayload(string payloadToValidate) {
            var isPayloadGood = false;
            var enumParts = _formatAttribute.Split(',');

            foreach (var part in enumParts) {
                if (part == payloadToValidate) { isPayloadGood = true; }
            }

            return isPayloadGood;
        }

        private void SetValue(string valueToSet) {
            switch (Type) {
                case PropertyType.State:
                case PropertyType.Parameter:
                    if (ValidatePayload(valueToSet) == true) {
                        _rawValue = valueToSet;
                        _parentDevice.InternalPropertyPublish($"{_propertyId}", _rawValue);
                    }

                    break;

                case PropertyType.Command:
                    throw new InvalidOperationException();
            }
        }
    }
}
