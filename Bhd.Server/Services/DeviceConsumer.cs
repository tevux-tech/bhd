using System;
using System.Diagnostics;
using DevBot9.Protocols.Homie;
using DevBot9.Protocols.Homie.Utilities;

namespace Bhd.Server.Services {
    public class DynamicConsumer : IDisposable {
        private PahoClientDeviceConnection _broker = new PahoClientDeviceConnection();

        public ClientDevice ClientDevice;

        public DynamicConsumer() { }

        public void Initialize(string mqttBrokerIpAddress, ClientDeviceMetadata clientDeviceMetadata) {
            ClientDevice = DeviceFactory.CreateClientDevice(clientDeviceMetadata);
            _broker.Initialize(mqttBrokerIpAddress);
            ClientDevice.Initialize(_broker, (severity, message) => { Console.WriteLine($"{severity}:{message}"); });
        }

        public void Dispose() {
            ClientDevice?.Dispose();
            _broker.Disconnect();
        }
    }
}
