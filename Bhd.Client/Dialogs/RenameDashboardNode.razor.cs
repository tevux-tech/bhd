using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Bhd.Shared.DTOs;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Bhd.Client.Dialogs {
    public partial class RenameDashboardNode {
        [CascadingParameter]
        MudDialogInstance MudDialog { get; set; }

        [Inject]
        ISnackbar Snackbar { get; set; }

        [Inject]
        private HttpClient HttpClient { get; set; }

        [Parameter]
        public string DashboardId { get; set; }

        [Parameter]
        public DashboardNode DashboardNode { get; set; }

        private string _newNodeName = "";

        protected override void OnParametersSet() {
            _newNodeName = DashboardNode.Name;
            base.OnParametersSet();
        }

        private void Cancel() {
            MudDialog.Cancel();
        }

        private async Task RenameNode() {
            var dashboardConfigs = await HttpClient.GetFromJsonAsync<List<DashboardConfig>>("api/dashboards/configuration");

            var dashboard = dashboardConfigs.FirstOrDefault(d => d.DashboardId == DashboardId);
            var node = dashboard?.Nodes.FirstOrDefault(n => n.NodeName == DashboardNode.Name);

            if (node != null) {
                node.NodeName = _newNodeName;
                await HttpClient.PutAsJsonAsync("api/dashboards/configuration", dashboardConfigs);
                Snackbar.Add($"Node renamed to \"{_newNodeName}\"", Severity.Success);
                MudDialog.Close(DialogResult.Ok(true));
            } else {
                Snackbar.Add($"Node \"{DashboardNode.Name}\" not found.", Severity.Error);
                MudDialog.Close(DialogResult.Ok(true));
            }
        }
    }
}
