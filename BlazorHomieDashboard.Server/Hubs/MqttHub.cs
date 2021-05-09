using BlazorHomieDashboard.Server.Services;
using Microsoft.AspNetCore.SignalR;
using NLog;

namespace BlazorHomieDashboard.Server.Hubs {
    class MqttHub : Hub {
        private readonly Logger _log = LogManager.GetCurrentClassLogger();

        private readonly IMqttBroker _mqttBroker;

        public MqttHub(IMqttBroker mqttBroker) {
            _mqttBroker = mqttBroker;
        }

        public void PublishToTopic(string topic, string payload, byte qosLevel, bool isRetained) {
            _mqttBroker.PublishToTopic(topic, payload, qosLevel, isRetained);
        }

        public void SubscribeToTopic(string topic) {
            _mqttBroker.SubscribeToTopic(topic);
        }
    }
}