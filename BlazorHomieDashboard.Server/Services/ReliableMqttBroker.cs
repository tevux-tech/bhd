using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using NLog;

namespace BlazorHomieDashboard.Server.Services {
    class ReliableMqttBroker : IMqttBroker, IDisposable {
        public event IMqttBroker.PublishReceivedDelegate PublishReceived;
        public bool IsConnected { get; private set; }

        public ReliableMqttBroker() {
            _cancellationTokenSource = new CancellationTokenSource();

            _mqttBrokerIp = Environment.GetEnvironmentVariable("MQTT_SERVER") ?? "127.0.0.1";
            _mqttBrokerPort = int.Parse(Environment.GetEnvironmentVariable("MQTT_SERVER_PORT") ?? "1883");

            var factory = new MqttFactory();
            _mqttClient = factory.CreateMqttClient();

            _mqttClient.UseApplicationMessageReceivedHandler(HandlePublishReceived);
            _mqttClient.UseDisconnectedHandler(h => {
                _log.Error("MQTTNet library reports a ConnectionClosed event.");
                IsConnected = false;
            });

            Task.Run(async () => await MonitorMqttConnectionContinuously(_cancellationTokenSource.Token));
        }

        public async Task MonitorMqttConnectionContinuously(CancellationToken cancellationToken) {
            while (cancellationToken.IsCancellationRequested == false) {
                if (IsConnected == false) {
                    try {
                        var options = new MqttClientOptionsBuilder().WithClientId(_mqttClientGuid).WithTcpServer(_mqttBrokerIp, _mqttBrokerPort).Build();
                        await _mqttClient.ConnectAsync(options, cancellationToken);

                        IsConnected = true;
                    } catch (Exception ex) {
                        _log.Error(ex, $"{nameof(MonitorMqttConnectionContinuously)} tried to connect to broker, but that did not work.");
                    }
                }

                await Task.Delay(1000, cancellationToken);
            }
        }

        public void PublishToTopic(string topic, string payload, byte qosLevel, bool isRetained) {
            if (IsConnected == false) { return; }

            var retryCount = 0;
            var isPublishSuccessful = false;
            while ((retryCount < 3) && (isPublishSuccessful == false)) {
                try {
                    _mqttClient.PublishAsync(new MqttApplicationMessage { Topic = topic, Payload = Encoding.UTF8.GetBytes(payload), QualityOfServiceLevel = (MQTTnet.Protocol.MqttQualityOfServiceLevel)qosLevel, Retain = isRetained }).Wait();
                    isPublishSuccessful = true;
                } catch (Exception ex) {
                    retryCount++;
                    _log.Error(ex, $"Could not publish topic {topic} to broker {_mqttBrokerIp}, attempt {retryCount}");
                }
            }

            if (isPublishSuccessful == false) {
                _log.Error($"Too many fails at publishing, going to disconnected state.");
                IsConnected = false;
            }
        }

        public void SubscribeToTopic(string topic) {
            _mqttClient.SubscribeAsync(topic, (MQTTnet.Protocol.MqttQualityOfServiceLevel)2);
        }

        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly Logger _log = LogManager.GetCurrentClassLogger();
        private readonly IMqttClient _mqttClient;
        private readonly string _mqttBrokerIp;
        private readonly int _mqttBrokerPort;
        private readonly string _mqttClientGuid = Guid.NewGuid().ToString();

        private void HandlePublishReceived(MqttApplicationMessageReceivedEventArgs e) {
            if (e.ApplicationMessage.Payload != null) {
                PublishReceived?.Invoke(e.ApplicationMessage.Topic, Encoding.UTF8.GetString(e.ApplicationMessage.Payload));
            } else {
                _log.Error($"Topic {e.ApplicationMessage.Topic} payload is null.");
            }
        }

        public void Dispose() {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _mqttClient?.Dispose();
        }
    }
}