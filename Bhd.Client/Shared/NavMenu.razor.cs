using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Bhd.Client.Dialogs;
using Bhd.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using MudBlazor;

namespace Bhd.Client.Shared {
    public partial class NavMenu {
        private List<Device> _devices = new();
        private List<Dashboard> _dashboards = new();

        [Inject]
        private HttpClient HttpClient { get; set; }

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

        private async Task LoadDevices() {
            _devices = await HttpClient.GetFromJsonAsync<List<Device>>("api/devices");
        }

        private async Task LoadDashboards() {
            _dashboards = await HttpClient.GetFromJsonAsync<List<Dashboard>>("api/dashboards");
        }

        private async Task HandleCreateDashboardClick(MouseEventArgs args) {
            var dialog = DialogService.Show<AddDashboard>();
            await dialog.Result;
        }

        private async Task HandleRescanButtonClick(MouseEventArgs args) {
            _isScanning = true;
            await HttpClient.PostAsync("api/devices/rescan", new StringContent(""));
            _isScanning = false;
        }
    }
}