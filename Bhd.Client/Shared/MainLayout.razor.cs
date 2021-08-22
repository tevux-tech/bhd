using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace Bhd.Client.Shared {
    public partial class MainLayout : LayoutComponentBase {
        bool _drawerOpen = true;

        [Inject]
        private PageHeaderService HeaderService { get; set; }

        protected override Task OnInitializedAsync() {
            HeaderService.PropertyChanged += (sender, args) => {
                StateHasChanged();
            };

            return base.OnInitializedAsync();
        }

        void DrawerToggle() {
            _drawerOpen = !_drawerOpen;
        }
    }
}