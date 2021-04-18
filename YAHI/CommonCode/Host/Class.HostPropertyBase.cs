namespace DevBot9.Protocols.Homie {
    /// <summary>
    /// A base class for the Host properties. Should not be consumed directly.
    /// </summary>
    public class HostPropertyBase : PropertyBase {
        /// <summary>
        /// Logical type of the property. This is NOT defined by Homie convention, but rather and additional constrain added by YAHI. However, it is fully Homie-compliant.
        /// </summary>
        public PropertyType Type { get; protected set; } = PropertyType.State;

        protected string _rawValue = "";
        protected readonly string _nameAttribute;
        protected readonly DataType _dataTypeAttribute;
        protected string _formatAttribute;
        protected readonly bool _isSettableAttribute;
        protected readonly bool _isRetainedAttribute;
        protected readonly string _unitAttribute;

        protected HostPropertyBase(PropertyType propertyType, string propertyId, string friendlyName, DataType dataType, string format, string unit) {
            Type = propertyType;

            _propertyId = propertyId;
            _nameAttribute = friendlyName;
            _dataTypeAttribute = dataType;
            _formatAttribute = format;
            _unitAttribute = unit;

            switch (Type) {
                case PropertyType.State:
                    _isRetainedAttribute = true;
                    _isSettableAttribute = false;
                    break;

                case PropertyType.Parameter:
                    _isRetainedAttribute = true;
                    _isSettableAttribute = true;
                    break;

                case PropertyType.Command:
                    _isRetainedAttribute = false;
                    _isSettableAttribute = true;
                    break;
            }
        }

        internal override void Initialize(Device parentDevice) {
            _parentDevice = parentDevice;

            _parentDevice.InternalPropertyPublish($"{_propertyId}/$name", _nameAttribute);
            _parentDevice.InternalPropertyPublish($"{_propertyId}/$datatype", _dataTypeAttribute.ToHomiePayload());
            _parentDevice.InternalPropertyPublish($"{_propertyId}/$format", _formatAttribute);
            _parentDevice.InternalPropertyPublish($"{_propertyId}/$settable", _isSettableAttribute.ToString().ToLower());
            _parentDevice.InternalPropertyPublish($"{_propertyId}/$retained", _isRetainedAttribute.ToString().ToLower());
            _parentDevice.InternalPropertyPublish($"{_propertyId}/$unit", _unitAttribute);
            _parentDevice.InternalPropertyPublish($"{_propertyId}", _rawValue);

            if (Type == PropertyType.Parameter) {
                _parentDevice.InternalPropertySubscribe($"{_propertyId}/set", (payload) => {
                    if (ValidatePayload(payload) == true) {
                        _rawValue = payload;

                        RaisePropertyChanged(this, new PropertyChangedEventArgs("Value"));

                        _parentDevice.InternalPropertyPublish($"{_propertyId}", _rawValue);
                    }
                });
            }

            if (Type == PropertyType.Command) {
                _parentDevice.InternalPropertySubscribe($"{_propertyId}/set", (payload) => {
                    if (ValidatePayload(payload) == true) {
                        _rawValue = payload;

                        RaisePropertyChanged(this, new PropertyChangedEventArgs("Value"));
                    }
                });
            }
        }

        protected virtual bool ValidatePayload(string payloadToValidate) {
            // This method must be overloaded by childs, so it doesn't really matter what it returns.
            return false;
        }
    }
}
