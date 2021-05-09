using System.Collections.Generic;
using System.Threading.Tasks;
using BlazorHomieDashboard.Server.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace BlazorHomieDashboard.Server.Hubs {
    class HomieHub : Hub {
        private readonly IHomieMqttService _homieMqttService;
        private readonly ILogger<HomieHub> _logger;

        public HomieHub(IHomieMqttService homieMqttService, ILogger<HomieHub> logger) {
            _homieMqttService = homieMqttService;
            _logger = logger;
        }

        public override async Task OnConnectedAsync() {
            var topicsDump = new List<string>();
            foreach (var item in _homieMqttService.HomieTopicsCache) {
                topicsDump.Add(item.Key + ":" + item.Value);
            }

            await Clients.Caller.SendAsync("CreateDashboard", topicsDump);

            foreach (var cacheItem in _homieMqttService.HomieTopicsCache) {
                await Clients.Caller.SendAsync("PublishReceived", cacheItem.Key, cacheItem.Value);
            }

            await base.OnConnectedAsync();
        }

        public async Task PublishToTopic(string topic, string payload, byte qosLevel, bool isRetained) {
            _logger.LogInformation($"Publishing \"{topic}\" to \"{payload}\" [Q{qosLevel}{(isRetained ? ", R" : "")}]");
            await _homieMqttService.PublishToTopicAsync(topic, payload, qosLevel, isRetained);
        }
    }
}