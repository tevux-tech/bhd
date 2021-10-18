using System;
using System.Collections.Generic;
using Bhd.Server.Hubs;
using DevBot9.Protocols.Homie;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Tevux.Protocols.Mqtt;

namespace Bhd.Server.Services {
    public class HomieService {
        private readonly ILogger<HomieService> _logger;

        public List<DynamicConsumer> DynamicConsumers = new();

        private readonly HomieTopicFetcher _fetcher;
        private readonly IHubContext<NotificationsHub> _notificationsHub;

        private readonly string _brokerIp;
        private readonly string _baseTopic;

        public HomieService(ILogger<HomieService> logger, IHubContext<NotificationsHub> notificationsHub) {
            _logger = logger;
            _notificationsHub = notificationsHub;

            _brokerIp = Environment.GetEnvironmentVariable("MQTT_SERVER") ?? "127.0.0.1";
            _baseTopic = Environment.GetEnvironmentVariable("BASE_TOPIC") ?? "homie";

            _logger.LogInformation($"MQTT_SERVER is \"{_brokerIp}\"");
            _logger.LogInformation($"BASE_TOPIC is \"{_baseTopic}\"");

            DeviceFactory.Initialize(_baseTopic);
            _fetcher = new HomieTopicFetcher();

            var options = new ChannelConnectionOptions();
            options.SetHostname(_brokerIp);
            _fetcher.Initialize(options);

            Rescan();
        }

        public void Rescan() {
            _logger.LogInformation("Rescanning...");

            foreach (var dynamicConsumer in DynamicConsumers) {
                dynamicConsumer.Dispose();
            }

            _fetcher.FetchDevices(DeviceFactory.BaseTopic, out var topicDump);

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
                            await _notificationsHub.Clients.All.SendAsync("DevicePropertyChanged", $"/api/devices/{deviceId}/nodes/{clientDeviceNode.NodeId}/properties/{propertyId}");
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
