using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using NLog;

namespace BlazorHomieDashboard.Server {
    class ReliableBroker {
        public delegate void PublishReceivedDelegate(string topic, string payload);

        public event PublishReceivedDelegate PublishReceived;
        public bool IsConnected { get; private set; }
        public bool IsInitialized { get; private set; }

        public void Initialize(string mqttBrokerIpAddress) {
            if (IsInitialized) { return; }

            _globalCancellationTokenSource = new CancellationTokenSource();

            _mqttBrokerIp = mqttBrokerIpAddress;

            var factory = new MqttFactory();
            _mqttClient = factory.CreateMqttClient();

            _mqttClient.UseApplicationMessageReceivedHandler(e => HandlePublishReceived(e));
            _mqttClient.UseDisconnectedHandler(h => {
                _log.Error("MQTTNet library reports a ConnectionClosed event.");
                IsConnected = false;
            });

            var options = new MqttClientOptionsBuilder().WithClientId(_mqttClientGuid).WithTcpServer(_mqttBrokerIp, 1883).Build();
            _mqttClient.ConnectAsync(options, CancellationToken.None).Wait();
            IsConnected = true;

            Task.Run(async () => await MonitorMqttConnectionContinuously(_globalCancellationTokenSource.Token));

            IsInitialized = true;
        }

        public async Task MonitorMqttConnectionContinuously(CancellationToken cancellationToken) {
            while (cancellationToken.IsCancellationRequested == false) {
                if (IsConnected == false) {
                    try {
                        var options = new MqttClientOptionsBuilder().WithClientId(_mqttClientGuid).WithTcpServer(_mqttBrokerIp, 1883).Build();
                        _mqttClient.ConnectAsync(options, CancellationToken.None).Wait(cancellationToken);

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

        private CancellationTokenSource _globalCancellationTokenSource;
        private readonly Logger _log = LogManager.GetCurrentClassLogger();
        private IMqttClient _mqttClient;
        private string _mqttBrokerIp = "localhost";
        private readonly string _mqttClientGuid = Guid.NewGuid().ToString();

        private void HandlePublishReceived(MqttApplicationMessageReceivedEventArgs e) {
            PublishReceived?.Invoke(e.ApplicationMessage.Topic, Encoding.UTF8.GetString(e.ApplicationMessage.Payload));
        }
    }
}