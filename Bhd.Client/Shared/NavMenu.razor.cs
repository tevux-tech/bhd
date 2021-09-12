using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Bhd.Client.Dialogs;
using Bhd.Client.Services;
using Bhd.Client.SignalR;
using Bhd.Shared.DTOs;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using MudBlazor;

namespace Bhd.Client.Shared {
    public partial class NavMenu {
        private List<Device> _devices = new();
        private List<Dashboard> _dashboards = new();

        [Inject]
        private IRestService RestService { get; set; }

        [Inject]
        private ILogger<NavMenu> Logger { get; set; }

        [Inject]
        private NotificationsHub NotificationsHub { get; set; }

        [Inject]
        private IDialogService DialogService { get; set; }

        private bool _isScanning = false;

        protected override async Task OnInitializedAsync() {
            NotificationsHub.DeviceStateChanged += async _ => {
                await LoadDevices();
                StateHasChanged();
            };

            NotificationsHub.DashboardConfigurationChanged += async () => {
                await LoadDashboards();
                StateHasChanged();
            };

            await LoadDevices();
            await LoadDashboards();
            await base.OnInitializedAsync();
        }

        private IEnumerable<Device> GetReadyDevices() {
            return _devices.Where(d => d.State == DeviceState.Ready).OrderBy(d => d.Id);
        }

        private IEnumerable<Device> GetAlertDevices() {
            return _devices.Where(d => d.State == DeviceState.Alert).OrderBy(d => d.Id);
        }

        private IEnumerable<Device> GetLostDevices() {
            return _devices.Where(d => d.State == DeviceState.Lost).OrderBy(d => d.Id);
        }

        private async Task LoadDevices() {
            var devicesResponse = await RestService.GetAsync<List<Device>>("api/devices");
            _devices = devicesResponse.Body;
        }

        private async Task LoadDashboards() {
            var dashboardsResponse = await RestService.GetAsync<List<Dashboard>>("api/dashboards");
            _dashboards = dashboardsResponse.Body;
        }

        private async Task HandleCreateDashboardClick(MouseEventArgs args) {
            var dialog = DialogService.Show<AddDashboard>();
            await dialog.Result;
        }

        private async Task HandleRescanButtonClick(MouseEventArgs args) {
            _isScanning = true;
            await RestService.PostAsync("api/devices/rescan", new StringContent(""));
            _isScanning = false;
        }
    }
}