using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BlazorHomieDashboard.Server.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;

namespace BlazorHomieDashboard.Server.Services {
    class HomieMqttService : IDisposable, IHomieMqttService {
        private readonly ILogger<HomieMqttService> _logger;
        private readonly IHubContext<HomieHub> _homieHubContext;
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private readonly IMqttClient _mqttClient;
        private readonly IMqttClientOptions _mqttClientOptions;
        private ConcurrentDictionary<string, string> _topicsCache = new();

        public HomieMqttService(ILogger<HomieMqttService> logger, IHubContext<HomieHub> homieHubContext) {
            _logger = logger;
            _homieHubContext = homieHubContext;
            _mqttClient = new MqttFactory().CreateMqttClient();
            _mqttClient.UseApplicationMessageReceivedHandler(HandlePublishReceivedAsync);

            var brokerIp = Environment.GetEnvironmentVariable("MQTT_SERVER") ?? "127.0.0.1";
            var brokerPort = int.Parse(Environment.GetEnvironmentVariable("MQTT_SERVER_PORT") ?? "1883");

            // If two clients with same id connects to mosquitto broker, previous connection gets closed by the broker. Therefore I generate random ID here.
            var uniqueClientId = "BHD-" + Guid.NewGuid().ToString().Substring(0, 8);

            _mqttClientOptions = new MqttClientOptionsBuilder().WithClientId(uniqueClientId).WithTcpServer(brokerIp, brokerPort).WithCleanSession().Build();

            Task.Run(async () => {
                try {
                    await MonitorConnectionContinuously(_cancellationTokenSource.Token);
                } catch (Exception ex) {
                    logger.LogCritical(ex, "Monitoring task failed");
                } finally {
                    logger.LogInformation("Monitoring task stopped");
                }
            });
        }

        public List<string> GetTopicsCache() {
            var topicsCache = new List<string>();
            foreach (var item in _topicsCache) {
                topicsCache.Add(item.Key + ":" + item.Value);
            }

            return topicsCache;
        }

        public async Task PublishToTopicAsync(string topic, string payload, byte qosLevel, bool isRetained) {
            if (_mqttClient.IsConnected == false) {
                _logger.LogError($"Broker not connected, refusing to publish \"{topic}\"");
                return;
            }

            _logger.LogInformation($"Publishing \"{topic}\" to \"{payload}\" [Q{qosLevel}{(isRetained ? ", R" : "")}]");

            try {
                await _mqttClient.PublishAsync(new MqttApplicationMessage { Topic = topic, Payload = Encoding.UTF8.GetBytes(payload), QualityOfServiceLevel = (MQTTnet.Protocol.MqttQualityOfServiceLevel)qosLevel, Retain = isRetained });
                _topicsCache[topic] = payload;
            } catch (Exception ex) {
                _logger.LogError(ex, $"Failed publishing topic \"{topic}\"");
            }
        }

        private async Task MonitorConnectionContinuously(CancellationToken cancellationToken) {
            while (cancellationToken.IsCancellationRequested == false) {
                if (_mqttClient.IsConnected == false) {
                    try {
                        await _mqttClient.ConnectAsync(_mqttClientOptions, cancellationToken);
                    } catch (Exception ex) {
                        _logger.LogError(ex, "Can't connect to MQTT server.");
                    }

                    if (_mqttClient.IsConnected) {
                        _topicsCache.Clear();
                        await SubscribeToTopicAsync("homie/#");
                    }
                }

                await Task.Delay(5000, cancellationToken);
            }
        }


        private async Task SubscribeToTopicAsync(string topic) {
            if (_mqttClient.IsConnected == false) {
                _logger.LogError($"Broker not connected, refusing to subscribe \"{topic}\"");
            }

            try {
                await _mqttClient.SubscribeAsync(topic, (MQTTnet.Protocol.MqttQualityOfServiceLevel)2);
            } catch (Exception ex) {
                _logger.LogError(ex, $"Failed subscribing to \"{topic}\"");
            }
        }

        private async Task HandlePublishReceivedAsync(MqttApplicationMessageReceivedEventArgs e) {
            var previousCacheSize = _topicsCache.Count;

            var topic = e.ApplicationMessage.Topic;
            string payload = null;

            if (e.ApplicationMessage.Payload != null) {
                payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
            }

            if (payload != null) {
                _topicsCache[e.ApplicationMessage.Topic] = payload;
                await _homieHubContext.Clients.All.SendAsync("PublishReceived", e.ApplicationMessage.Topic, payload);
            } else {
                var isRemoved = _topicsCache.Remove(e.ApplicationMessage.Topic, out _);
                if (isRemoved) {
                    _logger.LogInformation($"Removed \"{e.ApplicationMessage.Topic}\" from cache");
                }
            }

            if (topic.EndsWith("$state") && payload == "ready") {
                _logger.LogInformation($"{topic} is ready. Recreating dashboards");
                await _homieHubContext.Clients.All.SendAsync("CreateDashboard", GetTopicsCache());
            }

            if (topic.EndsWith("$state") && payload == null) {
                _logger.LogInformation($"{topic} is removed. Recreating dashboards");
                await _homieHubContext.Clients.All.SendAsync("CreateDashboard", GetTopicsCache());
            }

            if (_topicsCache.Count != previousCacheSize) {
                _logger.LogInformation($"Cache size changed to {_topicsCache.Count}");
            }
        }

        public void Dispose() {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
        }
    }
}