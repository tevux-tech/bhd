using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Bhd.Shared.DTOs;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Bhd.Client.Dialogs {
    public partial class Information {
        [CascadingParameter]
        MudDialogInstance MudDialog { get; set; }

        [Inject]
        private HttpClient HttpClient { get; set; }

        private List<Version> _versions = new();

        protected async override Task OnInitializedAsync() {
            MudDialog.Options.CloseButton = true;
            MudDialog.SetOptions(MudDialog.Options);
            _versions = await HttpClient.GetFromJsonAsync<List<Version>>("api/Versions");
            await base.OnInitializedAsync();
        }
    }
}