using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;

namespace BlazorHomieDashboard.Server.Services {
    class ReliableMqttBroker : IMqttBroker, IDisposable {
        private readonly ILogger<ReliableMqttBroker> _logger;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly IMqttClient _mqttClient;
        private readonly string _mqttBrokerIp;
        private readonly int _mqttBrokerPort;
        private readonly string _mqttClientGuid = Guid.NewGuid().ToString();

        public event IMqttBroker.PublishReceivedDelegate PublishReceived;
        public bool IsConnected { get; private set; }

        public ReliableMqttBroker(ILogger<ReliableMqttBroker> logger) {
            _logger = logger;

            _cancellationTokenSource = new CancellationTokenSource();

            _mqttBrokerIp = Environment.GetEnvironmentVariable("MQTT_SERVER") ?? "127.0.0.1";
            _mqttBrokerPort = int.Parse(Environment.GetEnvironmentVariable("MQTT_SERVER_PORT") ?? "1883");

            var factory = new MqttFactory();
            _mqttClient = factory.CreateMqttClient();

            _mqttClient.UseApplicationMessageReceivedHandler(HandlePublishReceived);
            _mqttClient.UseDisconnectedHandler(h => {
                _logger.LogError("MQTTNet library reports a ConnectionClosed event.");
                IsConnected = false;
            });

            Task.Run(async () => await MonitorMqttConnectionContinuously(_cancellationTokenSource.Token));
        }

        public async Task MonitorMqttConnectionContinuously(CancellationToken cancellationToken) {
            while (cancellationToken.IsCancellationRequested == false) {
                if (IsConnected == false) {
                    try {
                        _logger.LogInformation($"Connecting to {_mqttBrokerIp}:{_mqttBrokerPort}");

                        var options = new MqttClientOptionsBuilder().WithClientId(_mqttClientGuid).WithTcpServer(_mqttBrokerIp, _mqttBrokerPort).Build();
                        var result = await _mqttClient.ConnectAsync(options, cancellationToken);

                        _logger.LogInformation("Broker connected");

                        IsConnected = true;
                    } catch (Exception ex) {
                        _logger.LogError(ex, $"{nameof(MonitorMqttConnectionContinuously)} tried to connect to broker, but that did not work.");
                    }
                }

                await Task.Delay(1000, cancellationToken);
            }
        }

        public async Task PublishToTopicAsync(string topic, string payload, byte qosLevel, bool isRetained) {
            if (IsConnected == false) { return; }

            var retryCount = 0;
            var isPublishSuccessful = false;
            while ((retryCount < 3) && (isPublishSuccessful == false)) {
                try {
                    await _mqttClient.PublishAsync(new MqttApplicationMessage { Topic = topic, Payload = Encoding.UTF8.GetBytes(payload), QualityOfServiceLevel = (MQTTnet.Protocol.MqttQualityOfServiceLevel)qosLevel, Retain = isRetained });
                    isPublishSuccessful = true;
                } catch (Exception ex) {
                    retryCount++;
                    _logger.LogError(ex, $"Could not publish topic {topic} to broker {_mqttBrokerIp}, attempt {retryCount}");
                }
            }

            if (isPublishSuccessful == false) {
                _logger.LogError("Too many fails at publishing, going to disconnected state.");
                IsConnected = false;
            }
        }

        public async Task SubscribeToTopicAsync(string topic) {
            try {
                await _mqttClient.SubscribeAsync(topic, (MQTTnet.Protocol.MqttQualityOfServiceLevel)2);
            } catch (Exception ex) {
                IsConnected = false;
                _logger.LogError(ex, $"Failed subscribing to \"{topic}\"");
            }
        }

        private void HandlePublishReceived(MqttApplicationMessageReceivedEventArgs e) {
            if (e.ApplicationMessage.Payload != null) {
                PublishReceived?.Invoke(e.ApplicationMessage.Topic, Encoding.UTF8.GetString(e.ApplicationMessage.Payload));
            } else {
                _logger.LogWarning($"Skipping null payload in topic {e.ApplicationMessage.Topic}");
            }
        }

        public void Dispose() {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _mqttClient?.Dispose();
        }
    }
}