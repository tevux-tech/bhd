using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace Bhd.Client.SignalR {
    public class NotificationsHub : IDisposable {
        private readonly HubConnection _connection;
        private readonly HubReconnectPolicy _reconnectPolicy = new();

        public delegate void DeviceStateChangedDelegate(string deviceId);
        public delegate void ConfigurationChanged();
        public delegate void DevicePropertyChangedDelegate(string propertyPath);

        public event DeviceStateChangedDelegate DeviceStateChanged;
        public event DevicePropertyChangedDelegate DevicePropertyChanged;
        public event ConfigurationChanged DashboardConfigurationChanged;

        public bool IsReconnecting { get; private set; }

        public NotificationsHub(NavigationManager navigationManager) {
            _connection = new HubConnectionBuilder().WithUrl(navigationManager.ToAbsoluteUri("api/NotificationsHub")).WithAutomaticReconnect(_reconnectPolicy).Build();
            _connection.StartAsync();

            _connection.On("DeviceStateChanged", (string deviceId) => {
                DeviceStateChanged?.Invoke(deviceId);
            });

            _connection.On("DevicePropertyChanged", (string propertyPath) => {
                DevicePropertyChanged?.Invoke(propertyPath);
            });

            _connection.On("DashboardConfigurationChanged", () => {
                DashboardConfigurationChanged?.Invoke();
            });

            _connection.Reconnecting += (ex) => {
                IsReconnecting = true;
                return Task.CompletedTask;
            };

            _connection.Reconnected += s => {
                IsReconnecting = false;
                return Task.CompletedTask;
            };
        }

        public void Dispose() {
            _connection.DisposeAsync();
        }
    }
}
