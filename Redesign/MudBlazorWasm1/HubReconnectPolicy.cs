using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace MudBlazorWasm1 {
    public class HubReconnectPolicy : IRetryPolicy {
        public TimeSpan? NextRetryDelay(RetryContext retryContext) {
            return new TimeSpan(0, 0, 5);
        }
    }
}
