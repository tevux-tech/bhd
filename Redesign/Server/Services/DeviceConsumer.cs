using System;
using System.Diagnostics;
using DevBot9.Protocols.Homie;
using DevBot9.Protocols.Homie.Utilities;

namespace Server.Services {
    public class DynamicConsumer {
        private PahoClientDeviceConnection _broker = new PahoClientDeviceConnection();

        public ClientDevice ClientDevice;

        public DynamicConsumer() { }

        public void Initialize(string mqttBrokerIpAddress, ClientDeviceMetadata clientDeviceMetadata) {
            ClientDevice = DeviceFactory.CreateClientDevice(clientDeviceMetadata);

            for (var i = 0; i < ClientDevice.Nodes.Length; i++) {
                Debug.Print($"Iterating over nodes. Currently: \"{ClientDevice.Nodes[i].Name}\" with {ClientDevice.Nodes[i].Properties.Length} properties.");

                foreach (var property in ClientDevice.Nodes[i].Properties) {
                    property.PropertyChanged += (sender, e) => {
#warning Should I expose _rawValue property for read access?.. Otherwise I cannot access it as a  PropertyBase member...
                        // Debug.WriteLine($"Value of property \"{property.Name}\" changed to \"{property.Value}\".");
                    };

                }
            }

            // Initializing all the Homie stuff.
            _broker.Initialize(mqttBrokerIpAddress);
            ClientDevice.Initialize(_broker, (severity, message) => { Console.WriteLine($"{severity}:{message}"); });
        }
    }
}
