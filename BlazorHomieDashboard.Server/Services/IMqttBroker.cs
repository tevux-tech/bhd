using System.Threading.Tasks;

namespace BlazorHomieDashboard.Server.Services {
    interface IMqttBroker {
        public delegate void PublishReceivedDelegate(string topic, string payload);

        event PublishReceivedDelegate PublishReceived;
        Task PublishToTopicAsync(string topic, string payload, byte qosLevel, bool isRetained);
        Task SubscribeToTopicAsync(string topic);
    }
}