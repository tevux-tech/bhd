using System.Collections.Generic;
using System.Threading.Tasks;
using DevBot9.Protocols.Homie;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Server.Hubs;

namespace Server.Services {
    public class HomieService {
        private readonly ILogger<HomieService> _logger;

        public List<DynamicConsumer> DynamicConsumers = new();

        private HomieTopicFetcher _fetcher;
        private readonly IHubContext<NotificationsHub> _notificationsHub;

        public HomieService(ILogger<HomieService> logger, IHubContext<NotificationsHub> notificationsHub) {
            _logger = logger;
            _notificationsHub = notificationsHub;

            var brokerIp = "192.168.2.2";
            DeviceFactory.Initialize("homie");

            _fetcher = new HomieTopicFetcher();
            _fetcher.Initialize(brokerIp);
            _fetcher.FetchTopics(DeviceFactory.BaseTopic + "/#", out var topicDump);

            var homieTree = HomieTopicTreeParser.Parse(topicDump, DeviceFactory.BaseTopic, out var _, out var _);

            foreach (var clientDeviceMetadata in homieTree) {
                var consumer = new DynamicConsumer();
                consumer.Initialize(brokerIp, clientDeviceMetadata);

                var deviceId = consumer.ClientDevice.DeviceId;

                consumer.ClientDevice.PropertyChanged += async (sender, args) => {
                    await _notificationsHub.Clients.All.SendAsync("DeviceStateChanged", $"devices/{deviceId}");
                };

                foreach (var clientDeviceNode in consumer.ClientDevice.Nodes) {
                    var nodeId = clientDeviceNode.Name.Replace(" ", "-").ToLower();

                    foreach (var clientPropertyBase in clientDeviceNode.Properties) {
                        var propertyId = clientPropertyBase.PropertyId;

                        clientPropertyBase.PropertyChanged += async (sender, args) => {
                            await _notificationsHub.Clients.All.SendAsync("DevicePropertyChanged", $"devices/{deviceId}/{nodeId}/properties/{propertyId}");
                        };
                    }

                }

                DynamicConsumers.Add(consumer);
            }
        }
    }
}