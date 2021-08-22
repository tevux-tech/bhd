using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Bhd.Client.Dialogs;
using Bhd.Shared.DTOs;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Bhd.Client.Pages {
    public partial class Dashboard : IDisposable {
        [Parameter]
        public string DashboardId { get; set; }

        [Inject]
        private HttpClient HttpClient { get; set; }

        [Inject]
        private PageHeaderService PageHeaderService { get; set; }

        [Inject]
        private NotificationsHub NotificationsHub { get; set; }

        [Inject]
        private IDialogService DialogService { get; set; }

        [Inject] 
        private NavigationManager NavigationManager { get; set; }

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
            var dashboardConfigs = await HttpClient.GetFromJsonAsync<List<DashboardConfig>>("api/dashboards/configuration");
            dashboardConfigs?.RemoveAll(r => r.DashboardId == DashboardId);
            await HttpClient.PutAsJsonAsync("api/dashboards/configuration", dashboardConfigs);
            NavigationManager.NavigateTo("/");
        }

        private async Task AddNode() {
            var dialogParameters = new DialogParameters();
            dialogParameters["DashboardId"] = DashboardId;
            var result = await DialogService.Show<AddDashboardNode>(null, dialogParameters).Result;
        }

        private async Task RenameDashboard() {
            var dialogParameters = new DialogParameters();
            dialogParameters["Dashboard"] = _dashboard;
            var result = await DialogService.Show<RenameDashboard>(null, dialogParameters).Result;
        }

        private bool CanMoveUp(DashboardNode node, DashboardProperty property) {
            if (node.Properties.Count == 1) {
                return false;
            }

            if (node.Properties.IndexOf(property) == 0) {
                return false;
            }

            return true;
        }

        private bool CanMoveDown(DashboardNode node, DashboardProperty property) {
            if (node.Properties.Count == 1) {
                return false;
            }

            if (node.Properties.IndexOf(property) == node.Properties.Count - 1) {
                return false;
            }

            return true;
        }

        private async Task RemoveProperty(DashboardNode node, DashboardProperty property) {
            var dashboardConfigs = await HttpClient.GetFromJsonAsync<List<DashboardConfig>>("api/dashboards/configuration");
            var dashboardConfig = dashboardConfigs?.FirstOrDefault(d => d.DashboardId == DashboardId);
            var nodeConfig = dashboardConfig?.Nodes.FirstOrDefault(n => n.NodeName == node.Name);
            var propertyConfig = nodeConfig?.Properties.FirstOrDefault(p => p.PropertyPath == property.ActualPropertyPath);

            if (propertyConfig != null) {
                nodeConfig.Properties.Remove(propertyConfig);
                await HttpClient.PutAsJsonAsync("api/dashboards/configuration", dashboardConfigs);
            }
        }

        private async Task MoveProperty(DashboardNode node, DashboardProperty property, int offset) {
            var dashboardConfigs = await HttpClient.GetFromJsonAsync<List<DashboardConfig>>("api/dashboards/configuration");
            var dashboardConfig = dashboardConfigs?.FirstOrDefault(d => d.DashboardId == DashboardId);
            var nodeConfig = dashboardConfig?.Nodes.FirstOrDefault(n => n.NodeName == node.Name);
            var propertyConfig = nodeConfig?.Properties.FirstOrDefault(p => p.PropertyPath == property.ActualPropertyPath);

            if (propertyConfig != null) {
                var oldIndex = nodeConfig.Properties.IndexOf(propertyConfig);
                var newIndex = oldIndex + offset;
                nodeConfig.Properties.RemoveAt(oldIndex);
                nodeConfig.Properties.Insert(newIndex, propertyConfig);
                await HttpClient.PutAsJsonAsync("api/dashboards/configuration", dashboardConfigs);
            }
        }

        private async Task RemoveNode(DashboardNode node) {
            var dashboardConfigs = await HttpClient.GetFromJsonAsync<List<DashboardConfig>>("api/dashboards/configuration");
            var dashboardConfig = dashboardConfigs?.FirstOrDefault(d => d.DashboardId == DashboardId);
            var nodeConfig = dashboardConfig?.Nodes.FirstOrDefault(n => n.NodeName == node.Name);

            if (nodeConfig != null) {
                dashboardConfig.Nodes.Remove(nodeConfig);
                await HttpClient.PutAsJsonAsync("api/dashboards/configuration", dashboardConfigs);
            }
        }

        private async Task RenameProperty(DashboardNode node, DashboardProperty dashboardProperty) {
            var dialogParameters = new DialogParameters();
            dialogParameters["DashboardId"] = DashboardId;
            dialogParameters["DashboardNode"] = node;
            dialogParameters["DashboardProperty"] = dashboardProperty;
            var result = await DialogService.Show<RenameDashboardProperty>(null, dialogParameters).Result;
        }

        private async Task RenameNode(DashboardNode node) {
            var dialogParameters = new DialogParameters();
            dialogParameters["DashboardId"] = DashboardId;
            dialogParameters["DashboardNode"] = node;
            var result = await DialogService.Show<RenameDashboardNode>(null, dialogParameters).Result;
        }
    }
}