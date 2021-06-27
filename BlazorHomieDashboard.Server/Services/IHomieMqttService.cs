using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazorHomieDashboard.Server.Services {
    public interface IHomieMqttService {
        List<string> GetTopicsCache();
        Task PublishToTopicAsync(string topic, string payload, byte qosLevel, bool isRetained);
        Task RemoveDeviceTopics(string deviceId);
    }
}