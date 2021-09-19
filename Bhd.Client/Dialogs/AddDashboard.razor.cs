using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Bhd.Client.Services;
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
        private IRestService RestService { get; set; }

        private string _dashboardName = "";

        private void Cancel() {
            MudDialog.Cancel();
        }

        private async Task CreateDashboard() {
            var configResponse = await RestService.GetAsync<List<DashboardConfig>>("api/dashboards/configuration");

            var newConfig = configResponse.Body;
            newConfig.Add(new DashboardConfig { DashboardId = _dashboardName.Replace(" ", "-").Trim().ToLower(), DashboardName = _dashboardName });
            await RestService.PutAsync("api/dashboards/configuration", newConfig);

            Snackbar.Add(_dashboardName + " created", Severity.Success);
            MudDialog.Close(DialogResult.Ok(true));
        }
    }
}