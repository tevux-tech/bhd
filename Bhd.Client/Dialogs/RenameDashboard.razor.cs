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
    public partial class RenameDashboard {
        [CascadingParameter]
        MudDialogInstance MudDialog { get; set; }

        [Inject]
        ISnackbar Snackbar { get; set; }

        [Inject]
        private IRestService RestService { get; set; }

        [Inject]
        private NavigationManager NavigationManager { get; set; }

        [Parameter]
        public Dashboard Dashboard { get; set; }

        private string _newDashboardName = "";

        protected override void OnParametersSet() {
            _newDashboardName = Dashboard.Name;
            base.OnParametersSet();
        }

        private void Cancel() {
            MudDialog.Cancel();
        }

        private async Task Rename() {
            var dashboardConfigs = await RestService.GetAsync<List<DashboardConfig>>("api/dashboards/configuration");
            var dashboardConfig = dashboardConfigs?.FirstOrDefault(d => d.DashboardId == Dashboard.Id);

            if (dashboardConfig != null) {
                dashboardConfig.DashboardName = _newDashboardName;
                dashboardConfig.DashboardId = _newDashboardName.Replace(" ", "-").Trim().ToLower();
                await RestService.PutAsync("api/dashboards/configuration", dashboardConfigs);
                Snackbar.Add($"Dashboard renamed to \"{_newDashboardName}\"", Severity.Success);
                MudDialog.Close(DialogResult.Ok(true));

                NavigationManager.NavigateTo($"dashboards/{dashboardConfig.DashboardId}");
            } else {
                Snackbar.Add($"Dashboard \"{Dashboard.Id}\" not found.", Severity.Error);
                MudDialog.Close(DialogResult.Ok(true));
            }
        }
    }
}
