using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;

namespace Bhd.Client.Dialogs {
    public partial class NumberPicker {
        [CascadingParameter]
        MudDialogInstance MudDialog { get; set; }

    
        [Parameter]
        public string Unit { get; set; }

        [Parameter]
        public double Value { get; set; }

        private void Cancel() {
            MudDialog.Cancel();
        }

        private void Set() {
            MudDialog.Close(DialogResult.Ok(Value));
        }

        public void HandleKeyDown(KeyboardEventArgs obj) {
            if (obj.Key == "Enter") {
                MudDialog.Close(DialogResult.Ok(Value));
            }
        }
    }
}
