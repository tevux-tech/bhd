using System.Collections.Generic;
using Bhd.Server.Hubs;
using DevBot9.Protocols.Homie;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Bhd.Server.Services {
    public class HomieService {
        private readonly ILogger<HomieService> _logger;

        public List<DynamicConsumer> DynamicConsumers = new();

        private HomieTopicFetcher _fetcher;
        private readonly IHubContext<NotificationsHub> _notificationsHub;

        private string _brokerIp = "192.168.2.2";
        private string _baseTopic = "homie";

        public HomieService(ILogger<HomieService> logger, IHubContext<NotificationsHub> notificationsHub) {
            _logger = logger;
            _notificationsHub = notificationsHub;
            DeviceFactory.Initialize(_baseTopic);
            _fetcher = new HomieTopicFetcher();
            _fetcher.Initialize(_brokerIp);

            Rescan();
        }

        public void Rescan() {
            foreach (var dynamicConsumer in DynamicConsumers) {
                dynamicConsumer.Dispose();
            }

            _fetcher.FetchTopics(DeviceFactory.BaseTopic + "/#", out var topicDump);

            var homieTree = HomieTopicTreeParser.Parse(topicDump, DeviceFactory.BaseTopic, out var _, out var _);

            var dynamicConsumers = new List<DynamicConsumer>();

            foreach (var clientDeviceMetadata in homieTree) {
                var consumer = new DynamicConsumer();
                consumer.Initialize(_brokerIp, clientDeviceMetadata);

                var deviceId = consumer.ClientDevice.DeviceId;

                consumer.ClientDevice.PropertyChanged += async (sender, args) => {
                    await _notificationsHub.Clients.All.SendAsync("DeviceStateChanged", $"{deviceId}");
                };

                foreach (var clientDeviceNode in consumer.ClientDevice.Nodes) {
                    foreach (var clientPropertyBase in clientDeviceNode.Properties) {
                        var propertyId = clientPropertyBase.PropertyId.Replace($"{clientDeviceNode.NodeId}/", "");

                        clientPropertyBase.PropertyChanged += async (sender, args) => {
                            await _notificationsHub.Clients.All.SendAsync("DevicePropertyChanged", $"devices/{deviceId}/nodes/{clientDeviceNode.NodeId}/properties/{propertyId}");
                        };
                    }
                }

                dynamicConsumers.Add(consumer);
            }

            DynamicConsumers = dynamicConsumers;
        }
    }
}