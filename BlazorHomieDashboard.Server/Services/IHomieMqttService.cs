using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazorHomieDashboard.Server.Services {
    internal interface IHomieMqttService {
        Dictionary<string, string> HomieTopicsCache { get; }
        Task PublishToTopicAsync(string topic, string payload, byte qosLevel, bool isRetained);
    }
}