using System;

namespace DevBot9.Protocols.Homie {
    /// <summary>
    /// This class provides Homie Color property implementation. It can transparently convert between HSV and RGB types of data.
    /// </summary>
    public class HomieColor {
        private double _redValue = 0;
        private double _greenValue = 0;
        private double _blueValue = 0;

        /// <summary>
        /// Parses RGB string and sets internal variables.
        /// </summary>
        public void SetRgb(string rgbString) {
            var colorParts = rgbString.Split(',');

            _redValue = int.Parse(colorParts[0]);
            _greenValue = int.Parse(colorParts[1]);
            _blueValue = int.Parse(colorParts[2]);
        }

        /// <summary>
        /// Parses HSV string and sets internal variables.
        /// </summary>
        public void SetHsv(string hsvString) {
            var colorParts = hsvString.Split(',');

            var hue = int.Parse(colorParts[0]);
            var saturation = int.Parse(colorParts[1]) / 100.0;
            var value = int.Parse(colorParts[2]) / 100.0;

            // Took conversion from Wikipedia https://en.wikipedia.org/wiki/HSL_and_HSV
            var chroma = saturation * value;
            var hPrime = hue / 60.0;
            var x = chroma * (1 - Math.Abs(hPrime % 2 - 1));

            var redPrime = 0.0;
            var greenPrime = 0.0;
            var bluePrime = 0.0;
            if (hPrime <= 1) {
                redPrime = chroma;
                greenPrime = x;
                bluePrime = 0;
            }
            else if (hPrime <= 2) {
                redPrime = x;
                greenPrime = chroma;
                bluePrime = 0;
            }
            else if (hPrime <= 3) {
                redPrime = 0;
                greenPrime = chroma;
                bluePrime = x;
            }
            else if (hPrime <= 4) {
                redPrime = 0;
                greenPrime = x;
                bluePrime = chroma;
            }
            else if (hPrime <= 5) {
                redPrime = x;
                greenPrime = 0;
                bluePrime = chroma;
            }
            else if (hPrime <= 6) {
                redPrime = chroma;
                greenPrime = 0;
                bluePrime = x;
            }

            var m = value - chroma;

            _redValue = (redPrime + m) * 255;
            _greenValue = (greenPrime + m) * 255;
            _blueValue = (bluePrime + m) * 255;
        }

        /// <summary>
        /// Returns color in RGB string format.
        /// </summary>
        public string ToRgbString() {
            var returnColor = "0,0,0";

            returnColor = $"{_redValue},{_greenValue},{_blueValue}";

            return returnColor;

        }

        /// <summary>
        /// Returns color in HSV string format.
        /// </summary>
        public string ToHsvString() {
            var xMax = Math.Max(_redValue, _greenValue);
            xMax = Math.Max(xMax, _blueValue);

            var xMin = Math.Min(_redValue, _greenValue);
            xMin = Math.Min(xMin, _blueValue);

            var chroma = xMax - xMin;
            var lightness = (xMax + xMin) / 2;
            var v = xMax;

            var hue = 0.0;
            var saturation = 0.0;
            var value = v;

            if (chroma == 0) {
                hue = 0;
            }
            if (v == _redValue) {
                hue = 60 * (0 + (_greenValue - _blueValue) / chroma);
            }
            if (v == _greenValue) {
                hue = 60 * (2 + (_blueValue - _redValue) / chroma);
            }
            if (v == _blueValue) {
                hue = 60 * (4 + (_redValue - _greenValue) / chroma);
            }

            if (v == 0) { saturation = 0; }
            else { saturation = chroma / v; }

            return $"{hue},{saturation},{value}";
        }
    }
}
