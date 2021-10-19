using System;
using DevBot9.Protocols.Homie;
using DevBot9.Protocols.Homie.Utilities;

namespace Bhd.Server.Services {
    public class DynamicConsumer : IDisposable {
        private YahiTevuxClientConnection _broker = new();

        public ClientDevice ClientDevice;

        public DynamicConsumer() { }

        public void Initialize(IClientDeviceConnection brokerConnection, ClientDeviceMetadata clientDeviceMetadata) {
            ClientDevice = DeviceFactory.CreateClientDevice(clientDeviceMetadata);
            ClientDevice.Initialize(brokerConnection);
        }

        public void Dispose() {
            ClientDevice?.Dispose();
            _broker.DisconnectAndWait();
        }
    }
}
