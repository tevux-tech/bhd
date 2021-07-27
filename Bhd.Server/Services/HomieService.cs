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
            _logger.LogInformation("Rescanning...");

            foreach (var dynamicConsumer in DynamicConsumers) {
                dynamicConsumer.Dispose();
            }

            _fetcher.FetchTopics(DeviceFactory.BaseTopic + "/#", out var topicDump);

            var parsedDeviceMetadata = HomieTopicTreeParser.Parse(topicDump, DeviceFactory.BaseTopic, out var errorList, out var warningsList);

            foreach (var warning in warningsList) {
                _logger.LogWarning(warning);
            }

            foreach (var error in errorList) {
                _logger.LogError(error);
            }

            var dynamicConsumers = new List<DynamicConsumer>();

            foreach (var clientDeviceMetadata in parsedDeviceMetadata) {
                _logger.LogInformation($"Creating device \"{clientDeviceMetadata.Id}\"");

                var consumer = new DynamicConsumer();
                consumer.Initialize(_brokerIp, clientDeviceMetadata);

                var deviceId = consumer.ClientDevice.DeviceId;

                consumer.ClientDevice.PropertyChanged += async (sender, args) => {
                    await _notificationsHub.Clients.All.SendAsync("DeviceStateChanged", $"{deviceId}");
                };

                foreach (var clientDeviceNode in consumer.ClientDevice.Nodes) {
                    _logger.LogInformation($"Device \"{clientDeviceMetadata.Id}\" has {clientDeviceNode.Properties.Length} properties in node \"{clientDeviceNode.NodeId}\"");

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

            _logger.LogInformation("Rescanning done.");
        }
    }
}