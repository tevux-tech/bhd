using System;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace Bhd.Client {
    public class NotificationsHub : IDisposable {
        private readonly HubConnection _connection;

        public delegate void DeviceStateChangedDelegate(string deviceId);
        public delegate void ConfigurationChanged();
        public delegate void DevicePropertyChangedDelegate(string propertyPath);

        public event DeviceStateChangedDelegate DeviceStateChanged;
        public event DevicePropertyChangedDelegate DevicePropertyChanged;
        public event ConfigurationChanged DashboardConfigurationChanged;

        public NotificationsHub(NavigationManager navigationManager) {
            _connection = new HubConnectionBuilder().WithUrl(navigationManager.ToAbsoluteUri("api/NotificationsHub")).WithAutomaticReconnect(new HubReconnectPolicy()).Build();
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
        }

        public void Dispose() {
            _connection.DisposeAsync();
        }
    }
}
