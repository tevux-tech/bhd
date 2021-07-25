using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace Bhd.Client {
    public class NotificationsHub {
        public HubConnection Connection { get; }

        public NotificationsHub(NavigationManager navigationManager) {
            Connection = new HubConnectionBuilder().WithUrl(navigationManager.ToAbsoluteUri("api/NotificationsHub")).WithAutomaticReconnect(new HubReconnectPolicy()).Build();
            Connection.StartAsync();
        }
    }
}
