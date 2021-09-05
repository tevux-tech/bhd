using Bhd.Client.Services;
using Microsoft.AspNetCore.Components;

namespace Bhd.Client.Pages {
    public partial class Index {
        [Inject]
        private PageHeaderService PageHeaderService { get; set; }

        protected override void OnParametersSet() {
            PageHeaderService.CurrentPageTitle = "";
            base.OnParametersSet();
        }
    }
}
