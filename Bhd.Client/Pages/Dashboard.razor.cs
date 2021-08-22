using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Bhd.Shared;
using Bhd.Shared.DTOs;
using Microsoft.AspNetCore.Components;

namespace Bhd.Client.Pages {
    public partial class Dashboard {
        [Parameter]
        public string DashboardId { get; set; }

        [Inject]
        private HttpClient HttpClient { get; set; }

        [Inject]
        private PageHeaderService PageHeaderService { get; set; }

        [Inject]
        private NotificationsHub NotificationsHub { get; set; }

        private Bhd.Shared.DTOs.Dashboard _dashboard = new();
        private List<DashboardNode> _nodes = new();

        protected override async Task OnInitializedAsync() {
            NotificationsHub.DashboardConfigurationChanged += HandleDashboardConfigurationChanged;
            await base.OnInitializedAsync();
        }

        private void HandleDashboardConfigurationChanged() {
            Task.Run(async () => {
                await LoadDashboard();
                await LoadNodes();
                StateHasChanged();
            });
        }

        protected override async Task OnParametersSetAsync() {
            await LoadDashboard();
            await LoadNodes();
            PageHeaderService.CurrentPageTitle = $"/ Dashboards / {_dashboard.Name}";
            await base.OnParametersSetAsync();
        }

        private async Task LoadDashboard() {
            _dashboard = await HttpClient.GetFromJsonAsync<Bhd.Shared.DTOs.Dashboard>($"api/dashboards/{DashboardId}");
        }

        private async Task LoadNodes() {
            _nodes = await HttpClient.GetFromJsonAsync<List<DashboardNode>>($"api/dashboards/{DashboardId}/nodes");
        }

        public void Dispose() {
            NotificationsHub.DashboardConfigurationChanged -= HandleDashboardConfigurationChanged;
        }

        private async Task RemoveDashboard() {
            var config = await HttpClient.GetFromJsonAsync<List<DashboardConfig>>("api/dashboards/configuration");
            config.RemoveAll(r => r.DashboardId == DashboardId);
            await HttpClient.PutAsJsonAsync("api/dashboards/configuration", config);
        }
    }
}