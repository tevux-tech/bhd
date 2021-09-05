using System;
using System.Globalization;
using System.Threading.Tasks;
using Bhd.Client.Dialogs;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Bhd.Client.Shared {
    public partial class MainLayout : LayoutComponentBase {
        bool _drawerOpen = true;

        [Inject]
        private PageHeaderService HeaderService { get; set; }

        [Inject]
        private IDialogService DialogService { get; set; }

        private string _dateTime;

        protected override Task OnInitializedAsync() {
            HeaderService.PropertyChanged += (sender, args) => {
                StateHasChanged();
            };

            Task.Run(async () => {
                while (true) {
                    _dateTime = DateTime.Now.ToString(CultureInfo.CurrentCulture);
                    StateHasChanged();
                    await Task.Delay(1000);
                }
            });

            return base.OnInitializedAsync();
        }

        private async Task OpenInformationDialog() {
            await DialogService.Show<Information>().Result;
        }

        void DrawerToggle() {
            _drawerOpen = !_drawerOpen;
        }
    }
}