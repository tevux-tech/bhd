using System;
using Microsoft.AspNetCore.SignalR.Client;

namespace Bhd.Client {
    public class HubReconnectPolicy : IRetryPolicy {
        public TimeSpan? NextRetryDelay(RetryContext retryContext) {
            return new TimeSpan(0, 0, 5);
        }
    }
}
