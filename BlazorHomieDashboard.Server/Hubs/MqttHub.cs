using System.Threading.Tasks;
using BlazorHomieDashboard.Server.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace BlazorHomieDashboard.Server.Hubs {
    class MqttHub : Hub {
        private readonly IMqttBroker _mqttBroker;
        private readonly ILogger<MqttHub> _logger;

        public MqttHub(IMqttBroker mqttBroker, ILogger<MqttHub> logger) {
            _mqttBroker = mqttBroker;
            _logger = logger;
        }

        public async Task PublishToTopic(string topic, string payload, byte qosLevel, bool isRetained) {
            _logger.LogInformation($"Publishing \"{topic}\" to \"{payload}\" [Q{qosLevel}{(isRetained ? ", R" : "")}]");
            await _mqttBroker.PublishToTopicAsync(topic, payload, qosLevel, isRetained);
        }

        public async Task SubscribeToTopic(string topic) {
            _logger.LogInformation($"Subscribing to \"{topic}\"");
            await _mqttBroker.SubscribeToTopicAsync(topic);
        }
    }
}