using System;
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
        private readonly IHubContext<HomieHub> _mqttHubContext;
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private readonly IMqttClient _mqttClient;
        private readonly IMqttClientOptions _mqttClientOptions;

        public Dictionary<string, string> HomieTopicsCache { get; } = new();

        public HomieMqttService(ILogger<HomieMqttService> logger, IHubContext<HomieHub> mqttHubContext) {
            _logger = logger;
            _mqttHubContext = mqttHubContext;
            _mqttClient = new MqttFactory().CreateMqttClient();
            _mqttClient.UseApplicationMessageReceivedHandler(HandlePublishReceivedAsync);

            var brokerIp = Environment.GetEnvironmentVariable("MQTT_SERVER") ?? "127.0.0.1";
            var brokerPort = int.Parse(Environment.GetEnvironmentVariable("MQTT_SERVER_PORT") ?? "1883");
            _mqttClientOptions = new MqttClientOptionsBuilder().WithTcpServer(brokerIp, brokerPort).Build();

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

        public async Task PublishToTopicAsync(string topic, string payload, byte qosLevel, bool isRetained) {
            if (_mqttClient.IsConnected == false) {
                _logger.LogError($"Broker not connected, refusing to publish \"{topic}\"");
            }

            try {
                await _mqttClient.PublishAsync(new MqttApplicationMessage { Topic = topic, Payload = Encoding.UTF8.GetBytes(payload), QualityOfServiceLevel = (MQTTnet.Protocol.MqttQualityOfServiceLevel)qosLevel, Retain = isRetained });
                HomieTopicsCache[topic] = payload;
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
                        await SubscribeToTopicAsync("homie/#");
                    }
                }

                await Task.Delay(1000, cancellationToken);
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
            if (e.ApplicationMessage.Payload != null) {
                try {
                    var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);

                    HomieTopicsCache[e.ApplicationMessage.Topic] = payload;

                    await _mqttHubContext.Clients.All.SendAsync("PublishReceived", e.ApplicationMessage.Topic, payload);
                } catch (Exception ex) {
                    _logger.LogError(ex, $"Failed processing \"{e.ApplicationMessage.Topic}\"");
                }
            } else {
                _logger.LogWarning($"Skipping null payload in topic {e.ApplicationMessage.Topic}");
            }
        }

        public void Dispose() {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
        }
    }
}