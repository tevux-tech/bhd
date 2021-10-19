using System;
using System.Collections.Generic;
using Bhd.Server.Hubs;
using DevBot9.Protocols.Homie;
using DevBot9.Protocols.Homie.Utilities;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using NLog;
using Tevux.Protocols.Mqtt;

namespace Bhd.Server.Services {
    public class HomieService {
        private readonly ILogger<HomieService> _logger;

        public List<ClientDevice> HomieClientDevices { get; private set; } = new();

        private readonly HomieTopicFetcher _fetcher;
        private readonly IHubContext<NotificationsHub> _notificationsHub;

        private readonly string _brokerIp;
        private readonly string _baseTopic;
        private readonly IClientDeviceConnection _brokerConnection = new YahiTevuxClientConnection();

        public HomieService(ILogger<HomieService> logger, IHubContext<NotificationsHub> notificationsHub) {
            _logger = logger;
            _notificationsHub = notificationsHub;

            _brokerIp = Environment.GetEnvironmentVariable("MQTT_SERVER") ?? "127.0.0.1";
            _baseTopic = Environment.GetEnvironmentVariable("BASE_TOPIC") ?? "homie";

            _logger.LogInformation($"MQTT_SERVER is \"{_brokerIp}\"");
            _logger.LogInformation($"BASE_TOPIC is \"{_baseTopic}\"");

            // Configure NLog.
            var config = new NLog.Config.LoggingConfiguration();
            var logconsole = new NLog.Targets.ColoredConsoleTarget("console");
            var logdebug = new NLog.Targets.DebuggerTarget("debugger");
            config.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, logconsole);
            config.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, logdebug);
            LogManager.Configuration = config;

            DeviceFactory.Initialize(_baseTopic);
            _fetcher = new HomieTopicFetcher();

            var options = new ChannelConnectionOptions();
            options.SetHostname(_brokerIp);
            _fetcher.Initialize(options);

            ((YahiTevuxClientConnection)_brokerConnection).Initialize(options);
            _brokerConnection.Connect();

            Rescan();
        }

        public void Rescan() {
            _logger.LogInformation("Rescanning...");

            foreach (var device in HomieClientDevices) {
                device.Dispose();
            }

            _fetcher.FetchDevices(DeviceFactory.BaseTopic, out var topicDump);

            var parsedDeviceMetadata = HomieTopicTreeParser.Parse(topicDump, DeviceFactory.BaseTopic, out var errorList, out var warningsList);

            foreach (var warning in warningsList) {
                _logger.LogWarning(warning);
            }

            foreach (var error in errorList) {
                _logger.LogError(error);
            }

            var newDeviceList = new List<ClientDevice>();

            foreach (var deviceMetadata in parsedDeviceMetadata) {
                _logger.LogInformation($"Creating device \"{deviceMetadata.Id}\"");

                var clientDevice = DeviceFactory.CreateClientDevice(deviceMetadata);
                clientDevice.Initialize(_brokerConnection);

                var deviceId = clientDevice.DeviceId;

                clientDevice.PropertyChanged += async (sender, args) => {
                    await _notificationsHub.Clients.All.SendAsync("DeviceStateChanged", $"{deviceId}");
                };

                foreach (var clientDeviceNode in clientDevice.Nodes) {
                    _logger.LogInformation($"Device \"{deviceMetadata.Id}\" has {clientDeviceNode.Properties.Length} properties in node \"{clientDeviceNode.NodeId}\"");

                    foreach (var clientPropertyBase in clientDeviceNode.Properties) {
                        var propertyId = clientPropertyBase.PropertyId.Replace($"{clientDeviceNode.NodeId}/", "");

                        clientPropertyBase.PropertyChanged += async (sender, args) => {
                            await _notificationsHub.Clients.All.SendAsync("DevicePropertyChanged", $"/api/devices/{deviceId}/nodes/{clientDeviceNode.NodeId}/properties/{propertyId}");
                        };
                    }
                }

                newDeviceList.Add(clientDevice);
            }

            HomieClientDevices = newDeviceList;

            _logger.LogInformation("Rescanning done.");
        }
    }
}
