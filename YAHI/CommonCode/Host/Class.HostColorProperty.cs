using System;

namespace DevBot9.Protocols.Homie {
    /// <summary>
    /// A property of type Color, as defined by the Homie convention.
    /// </summary>
    public class HostColorProperty : HostPropertyBase {
        /// <summary>
        /// Set value will be published to the MQTT broker. Getting the property will retrieve value from the cache.
        /// </summary>
        public HomieColor Value {
            get {
                var returnValue = new HomieColor();

                if (_format == ColorFormat.Rgb) { returnValue.SetRgb(_rawValue); }
                if (_format == ColorFormat.Hsv) { returnValue.SetHsv(_rawValue); }

                return returnValue;
            }
            set {
                SetValue(value);
            }
        }

        private ColorFormat _format = ColorFormat.Rgb;

        internal HostColorProperty(PropertyType propertyType, string propertyId, string friendlyName, ColorFormat format, string unit) : base(propertyType, propertyId, friendlyName, DataType.Color, format.ToHomiePayload(), unit) {
            _rawValue = "0,0,0";
            _format = format;
        }

        internal override void Initialize(Device parentDevice) {
            base.Initialize(parentDevice);
        }

        protected override bool ValidatePayload(string payloadToValidate) {
            var colorParts = payloadToValidate.Split(',');
            if (colorParts.Length != 3) { return false; }

            var areNumbersGood = true;
            if (_format == ColorFormat.Rgb) {
                if (Helpers.TryParseInt(colorParts[0], out var red)) {
                    if (red < 0) { areNumbersGood &= false; }
                    if (red > 255) { areNumbersGood &= false; }
                };
                if (Helpers.TryParseInt(colorParts[1], out var green)) {
                    if (green < 0) { areNumbersGood &= false; }
                    if (green > 255) { areNumbersGood &= false; }
                };
                if (Helpers.TryParseInt(colorParts[2], out var blue)) {
                    if (blue < 0) { areNumbersGood &= false; }
                    if (blue > 255) { areNumbersGood &= false; }
                }
            }
            if (_format == ColorFormat.Hsv) {
                if (Helpers.TryParseInt(colorParts[0], out var hue)) {
                    if (hue < 0) { areNumbersGood &= false; }
                    if (hue > 360) { areNumbersGood &= false; }
                };
                if (Helpers.TryParseInt(colorParts[1], out var saturation)) {
                    if (saturation < 0) { areNumbersGood &= false; }
                    if (saturation > 100) { areNumbersGood &= false; }
                };
                if (Helpers.TryParseInt(colorParts[2], out var value)) {
                    if (value < 0) { areNumbersGood &= false; }
                    if (value > 100) { areNumbersGood &= false; }
                }
            }

            return areNumbersGood;
        }

        private void SetValue(HomieColor valueToSet) {
            switch (Type) {
                case PropertyType.State:
                case PropertyType.Parameter:
                    if (_format == ColorFormat.Rgb) { _rawValue = valueToSet.ToRgbString(); }
                    if (_format == ColorFormat.Hsv) { _rawValue = valueToSet.ToHsvString(); }

                    _parentDevice.InternalPropertyPublish($"{_propertyId}", _rawValue);
                    break;

                case PropertyType.Command:
                    throw new InvalidOperationException();
            }
        }
    }
}
