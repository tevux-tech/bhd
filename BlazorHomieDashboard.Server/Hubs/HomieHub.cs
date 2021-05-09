using System;
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
            _logger.LogInformation("Client connected.");
            await Clients.Caller.SendAsync("CreateDashboard", _homieMqttService.GetTopicsCache());
            await base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception) {
            _logger.LogInformation(exception, "Client disconnected");
            return base.OnDisconnectedAsync(exception);
        }

        public async Task PublishToTopic(string topic, string payload, byte qosLevel, bool isRetained) {
            await _homieMqttService.PublishToTopicAsync(topic, payload, qosLevel, isRetained);
        }
    }
}