using System;

namespace DevBot9.Protocols.Homie {
    /// <summary>
    /// A base class for the Client properties. Should not be consumed directly.
    /// </summary>
    public class ClientPropertyBase : PropertyBase {
        /// <summary>
        /// Logical type of the property. This is NOT defined by Homie convention, but rather and additional constrain added by YAHI. However, it is fully Homie-compliant.
        /// </summary>
        public PropertyType Type { get; protected set; } = PropertyType.State;

        /// <summary>
        /// Friendly name of the property.
        /// </summary>
        public string Name {
            get { return _name; }
            protected set {
                _name = value;
                RaisePropertyChanged(this, new PropertyChangedEventArgs(nameof(Name)));
            }
        }

        /// <summary>
        /// Data type of the property, as defined by the Homie convention.
        /// </summary>
        public DataType DataType {
            get { return _dataType; }
            protected set {
                _dataType = value;
                RaisePropertyChanged(this, new PropertyChangedEventArgs(nameof(Format)));
            }
        }

        /// <summary>
        /// Format of the property, as (inconsistently) defined by the Homie convention.
        /// </summary>
        public string Format {
            get { return _format; }
            protected set {
                _format = value;
                RaisePropertyChanged(this, new PropertyChangedEventArgs(nameof(Format)));
            }
        }

        /// <summary>
        /// Unit of the property, as (softly) defined by the Homie convention.
        /// </summary>
        public string Unit {
            get { return _unit; }
            protected set {
                _unit = value;
                RaisePropertyChanged(this, new PropertyChangedEventArgs(nameof(Unit)));
            }
        }

        private string _name = "";
        private DataType _dataType = DataType.String;
        private string _format = "";
        private string _unit = "";
        private bool _isSettable = true;
        private bool _isRetained = true;
        protected string _rawValue = "";

        protected ClientPropertyBase(ClientPropertyMetadata creationOptions) {
            _propertyId = creationOptions.NodeId + "/" + creationOptions.PropertyId;
            Type = creationOptions.PropertyType;
            Name = creationOptions.Name;
            DataType = creationOptions.DataType;
            Format = creationOptions.Format;
            Unit = creationOptions.Unit;
            if (creationOptions.InitialValue != "") { _rawValue = creationOptions.InitialValue; }
        }

        internal override void Initialize(Device parentDevice) {
            _parentDevice = parentDevice;

            //_parentDevice.InternalPropertySubscribe($"{_propertyId}/$name", (value) => {
            //    if (parentDevice.GetIsInitializing()) { Name = value; }
            //});

            //_parentDevice.InternalPropertySubscribe($"{_propertyId}/$datatype", (value) => {
            //    if (parentDevice.GetIsInitializing()) { DataType = DataType.String; }
            //});

            //_parentDevice.InternalPropertySubscribe($"{_propertyId}/$format", (value) => {
            //    if (parentDevice.GetIsInitializing()) { Format = value; }
            //});

            //_parentDevice.InternalPropertySubscribe($"{_propertyId}/$settable", (value) => {
            //    if (parentDevice.GetIsInitializing()) {
            //        if (bool.TryParse(value, out _isSettable)) {
            //            DefineType(_isSettable, _isRetained);
            //        }
            //    }
            //});

            //_parentDevice.InternalPropertySubscribe($"{_propertyId}/$retained", (value) => {
            //    if (parentDevice.GetIsInitializing()) {
            //        if (bool.TryParse(value, out _isRetained)) {
            //            DefineType(_isSettable, _isRetained);
            //        }
            //    }
            //});

            //_parentDevice.InternalPropertySubscribe($"{_propertyId}/$unit", (value) => {
            //    if (parentDevice.GetIsInitializing() == false) { Unit = value; }
            //});


            if (Type == PropertyType.State) {
                _parentDevice.InternalPropertySubscribe($"{_propertyId}", (payload) => {
                    if (ValidatePayload(payload) == true) {
                        _rawValue = payload;

                        RaisePropertyChanged(this, new PropertyChangedEventArgs("Value"));
                    }
                });
            }

            if (Type == PropertyType.Parameter) {
                _parentDevice.InternalPropertySubscribe($"{_propertyId}/set", (payload) => {
                    if (ValidatePayload(payload) == true) {
                        _rawValue = payload;

                        RaisePropertyChanged(this, new PropertyChangedEventArgs("Value"));
                    }
                });
            }
        }
        protected virtual bool ValidatePayload(string payloadToValidate) {
            return false;
        }

        private PropertyType DefineType(bool isSettable, bool isRetained) {
            var returnType = PropertyType.State;

            if ((isSettable == false) && (isRetained == true)) { returnType = PropertyType.State; }
            if ((isSettable == true) && (isRetained == false)) { returnType = PropertyType.Command; }
            if ((isSettable == true) && (isRetained == true)) { returnType = PropertyType.Parameter; }
            if ((isSettable == false) && (isRetained == false)) { throw new ArgumentException("Not allowed by YAHI..."); }

            return returnType;
        }
    }
}
