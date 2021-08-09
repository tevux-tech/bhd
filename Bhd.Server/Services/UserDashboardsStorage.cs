using System.Collections.Generic;
using Bhd.Server.Hubs;
using Bhd.Shared;
using Microsoft.AspNetCore.SignalR;

namespace Bhd.Server.Services {
    public class UserDashboardsStorage {

        private List<DashboardConfig> _dashboards = new();
        public List<DashboardConfig> Dashboards {
            get { return _dashboards; }
            set {
                if (_dashboards == value) {
                    return;
                }

                _dashboards = value;

                _notificationsHub.Clients.All.SendAsync("DashboardConfigurationChanged");
            }
        }

        private readonly IHubContext<NotificationsHub> _notificationsHub;

        public UserDashboardsStorage(IHubContext<NotificationsHub> notificationsHub) {
            _notificationsHub = notificationsHub;
        }
    }
}
