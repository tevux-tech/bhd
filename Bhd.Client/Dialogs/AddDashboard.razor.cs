using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Bhd.Shared.DTOs;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Bhd.Client.Dialogs {
    public partial class AddDashboard {
        [CascadingParameter]
        MudDialogInstance MudDialog { get; set; }

        [Inject]
        ISnackbar Snackbar { get; set; }

        [Inject]
        private HttpClient HttpClient { get; set; }

        private string _dashboardName = "";

        private void Cancel() {
            MudDialog.Cancel();
        }

        private async Task CreateDashboard() {
            var config = await HttpClient.GetFromJsonAsync<List<DashboardConfig>>("api/dashboards/configuration");

            config.Add(new DashboardConfig { DashboardId = _dashboardName.Replace(" ", "-").Trim().ToLower(), DashboardName = _dashboardName });

            await HttpClient.PutAsJsonAsync("api/dashboards/configuration", config);

            Snackbar.Add(_dashboardName + " created", Severity.Success);
            MudDialog.Close(DialogResult.Ok(true));
        }
    }
}