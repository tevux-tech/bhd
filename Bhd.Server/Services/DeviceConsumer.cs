using System;
using System.Diagnostics;
using DevBot9.Protocols.Homie;
using DevBot9.Protocols.Homie.Utilities;
using Tevux.Protocols.Mqtt;

namespace Bhd.Server.Services {
    public class DynamicConsumer : IDisposable {
        private YahiTevuxClientConnection _broker = new();

        public ClientDevice ClientDevice;

        public DynamicConsumer() { }

        public void Initialize(string mqttBrokerIpAddress, ClientDeviceMetadata clientDeviceMetadata) {
            ClientDevice = DeviceFactory.CreateClientDevice(clientDeviceMetadata);

            var connectOptions = new ChannelConnectionOptions();
            connectOptions.SetHostname(mqttBrokerIpAddress);

            _broker.Initialize(connectOptions);
            ClientDevice.Initialize(_broker, (severity, message) => { Console.WriteLine($"{severity}:{message}"); });
        }

        public void Dispose() {
            ClientDevice?.Dispose();
            _broker.Disconnect();
        }
    }
}
