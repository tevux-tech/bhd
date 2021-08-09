using System.Linq;
using Bhd.Shared;
using DevBot9.Protocols.Homie;

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
                    property.Type = Shared.PropertyType.Number;
                    property.NumericValue = numberProperty.Value;
                    break;

                case ClientChoiceProperty choiceProperty:
                    property.TextValue = choiceProperty.Value;
                    property.Type = Shared.PropertyType.Choice;
                    property.Choices = choiceProperty.Format.Split(",").ToList();
                    break;

                case ClientTextProperty textProperty:
                    property.Type = Shared.PropertyType.Text;
                    property.TextValue = textProperty.Value;
                    break;
            }

            return property;
        }
    }
}
