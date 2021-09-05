using System.Globalization;
using System.Linq;
using Bhd.Shared;
using Bhd.Shared.DTOs;
using DevBot9.Protocols.Homie;
using PropertyType = Bhd.Shared.DTOs.PropertyType;

namespace Bhd.Server {
    public static class PropertyFactory {
        public static Property Create(ClientPropertyBase clientPropertyBase) {
            var property = new Property();
            property.Name = clientPropertyBase.Name;
            property.Format = clientPropertyBase.Format;
            property.Unit = clientPropertyBase.Unit;

            switch (clientPropertyBase.Type) {
                case DevBot9.Protocols.Homie.PropertyType.State:
                    property.Direction = Direction.Read;
                    break;

                case DevBot9.Protocols.Homie.PropertyType.Command:
                    property.Direction = Direction.Write;
                    break;

                case DevBot9.Protocols.Homie.PropertyType.Parameter:
                    property.Direction = Direction.ReadWrite;
                    break;
            }

            switch (clientPropertyBase) {
                case ClientNumberProperty numberProperty:
                    property.Type = PropertyType.Number;
                    property.NumericValue = numberProperty.Value;
                    break;

                case ClientChoiceProperty choiceProperty:
                    property.TextValue = choiceProperty.Value;
                    property.Type = PropertyType.Choice;
                    property.Choices = choiceProperty.Format.Split(",").ToList();
                    break;

                case ClientTextProperty textProperty:
                    property.Type = PropertyType.Text;
                    property.TextValue = textProperty.Value;
                    break;

                case ClientColorProperty colorProperty:
                    property.Type = PropertyType.Color;
                    property.TextValue = colorProperty.Value.ToRgbString();
                    break;

                case ClientDateTimeProperty dateTimeProperty:
                    property.Type = PropertyType.Text;
                    property.TextValue = dateTimeProperty.Value.ToString(CultureInfo.InvariantCulture);
                    break;
            }

            return property;
        }
    }
}
