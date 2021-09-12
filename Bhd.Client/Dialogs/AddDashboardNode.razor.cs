using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Bhd.Client.Services;
using Bhd.Shared.DTOs;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Bhd.Client.Dialogs {
    public partial class AddDashboardNode {
        [CascadingParameter]
        MudDialogInstance MudDialog { get; set; }

        [Inject]
        ISnackbar Snackbar { get; set; }

        [Inject]
        private IRestService RestService { get; set; }

        [Parameter]
        public string DashboardId { get; set; }

        private string _nodeName;

        private void Cancel() {
            MudDialog.Cancel();
        }

        private async Task Add() {
            if (_nodeName == null) {
                return;
            }

            var dashboardsResponse = await RestService.GetAsync<List<DashboardConfig>>("api/dashboards/configuration");
            var dashboards = dashboardsResponse.Body;

            var dashboardToModify = dashboards?.FirstOrDefault(d => d.DashboardId == DashboardId);

            if (dashboardToModify != null) {
                dashboardToModify.Nodes.Add(new NodeConfig() { NodeName = _nodeName });
                await RestService.PutAsync("api/dashboards/configuration", dashboards);
                Snackbar.Add($"\"{_nodeName}\" added to \"{dashboardToModify.DashboardName}\"", Severity.Success);
                MudDialog.Close(DialogResult.Ok(true));
            } else {
                Snackbar.Add($"Can't find dashboard \"{DashboardId}\"", Severity.Error);
                MudDialog.Close(DialogResult.Ok(false));
            }
        }
    }
}