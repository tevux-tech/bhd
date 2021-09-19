using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Bhd.Client.Dialogs;
using Bhd.Client.Services;
using Bhd.Client.SignalR;
using Bhd.Shared.DTOs;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Bhd.Client.Pages {
    public partial class Device : IDisposable {
        [Parameter]
        public string DeviceId { get; set; }

        [Inject]
        private IRestService RestService { get; set; }

        [Inject]
        private NotificationsHub NotificationsHub { get; set; }

        [Inject]
        private PageHeaderService PageHeaderService { get; set; }

        [Inject]
        private IDialogService DialogService { get; set; }

        private List<Node> _nodes = new();
        private Bhd.Shared.DTOs.Device _device = new();

        protected override async Task OnParametersSetAsync() {
            await LoadDeviceInfo();
            await LoadDeviceNodes();
            PageHeaderService.CurrentPageTitle = $"/ Devices / {_device.Name}";
            await base.OnParametersSetAsync();
        }

        protected override async Task OnInitializedAsync() {
            NotificationsHub.DeviceStateChanged += HandleDeviceStateChanged;
            await base.OnInitializedAsync();
        }

        private void HandleDeviceStateChanged(string deviceId) {
            Task.Run(async () => {
                if (deviceId == DeviceId) {
                    await LoadDeviceInfo();
                    await LoadDeviceNodes();
                    StateHasChanged();
                }
            });
        }

        private async Task LoadDeviceInfo() {
            var deviceResponse = await RestService.GetAsync<Bhd.Shared.DTOs.Device>($"api/devices/{DeviceId}");
            _device = deviceResponse.Body;
        }

        private async Task LoadDeviceNodes() {
            var nodesResponse = await RestService.GetAsync<List<Node>>($"api/devices/{DeviceId}/nodes");
            _nodes = nodesResponse.Body;
        }

        public void Dispose() {
            NotificationsHub.DeviceStateChanged -= HandleDeviceStateChanged;
        }

        private async Task AddToDashboard(string propertyPath) {
            var dialogParameters = new DialogParameters();
            dialogParameters["PropertyPath"] = propertyPath;
            var result = await DialogService.Show<AddToDashboard>(null, dialogParameters).Result;
        }
    }
}
