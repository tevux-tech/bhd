using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Bhd.Client.Services;
using Bhd.Shared.DTOs;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Bhd.Client.Dialogs {
    public partial class Information {
        [CascadingParameter]
        MudDialogInstance MudDialog { get; set; }

        [Inject]
        private IRestService RestService { get; set; }

        private List<Version> _versions = new();

        protected async override Task OnInitializedAsync() {
            MudDialog.Options.CloseButton = true;
            MudDialog.SetOptions(MudDialog.Options);

            var versionsResponse = await RestService.GetAsync<List<Version>>("api/Versions");
            _versions = versionsResponse.Body;
            await base.OnInitializedAsync();
        }
    }
}