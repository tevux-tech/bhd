using BlazorHomieDashboard.Server.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace BlazorHomieDashboard.Server.Services {
    class MessagePusher {
        public MessagePusher(IMqttBroker mqttBroker, IHubContext<MqttHub> mqttHub) {
            mqttBroker.PublishReceived += async (topic, payload) => {
                await mqttHub.Clients.All.SendAsync("PublishReceived", topic, payload);
            };
        }
    }
}