using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Bhd.Client.Services;
using Bhd.Shared.DTOs;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Bhd.Client.Dialogs {
    public partial class DeleteDashboard {
        [CascadingParameter]
        MudDialogInstance MudDialog { get; set; }

        [Parameter]
        public Dashboard Dashboard { get; set; }

        [Inject]
        private IRestService RestService { get; set; }

        [Inject]
        ISnackbar Snackbar { get; set; }

        private void Cancel() {
            MudDialog.Cancel();
        }

        private async Task Delete() {
            var dashboardConfigs = await RestService.GetAsync<List<DashboardConfig>>("api/dashboards/configuration");
            dashboardConfigs?.RemoveAll(r => r.DashboardId == Dashboard.Id);
            await RestService.PutAsync("api/dashboards/configuration", dashboardConfigs);
            Snackbar.Add($"Dashboard \"{Dashboard.Name}\" removed", Severity.Success);
            MudDialog.Close(DialogResult.Ok(true));
        }
    }
}
