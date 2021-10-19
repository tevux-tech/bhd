using System;
using DevBot9.Protocols.Homie;

namespace Bhd.Server.Services {
    public class DynamicConsumer : IDisposable {
        public ClientDevice ClientDevice { get; private set; }

        public void Initialize(IClientDeviceConnection brokerConnection, ClientDeviceMetadata clientDeviceMetadata) {
            ClientDevice = DeviceFactory.CreateClientDevice(clientDeviceMetadata);
            ClientDevice.Initialize(brokerConnection);
        }

        public void Dispose() {
            ClientDevice.Dispose();
        }
    }
}