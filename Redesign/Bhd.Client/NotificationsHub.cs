using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace Bhd.Client {
    public class NotificationsHub {
        private readonly HubConnection _connection;

        public NotificationsHub(NavigationManager navigationManager) {
            _connection = new HubConnectionBuilder().WithUrl(navigationManager.ToAbsoluteUri("api/NotificationsHub")).WithAutomaticReconnect(new HubReconnectPolicy()).Build();
            _connection.StartAsync();
        }

        public void OnDeviceStateChanged(Func<string, Task> handler) {
            _connection.On("DeviceStateChanged", handler);
        }

        public void OnDevicePropertyChanged(Func<string, Task> handler) {
            _connection.On("DevicePropertyChanged", handler);
        }
    }
}
