using Microsoft.AspNetCore.Components;
using MudBlazor;
using MudBlazor.Utilities;

namespace Bhd.Client.Dialogs {
    public partial class MyColorPicker {
        [CascadingParameter]
        MudDialogInstance MudDialog { get; set; }

        [Parameter]
        public string ActualColor { get; set; }
        private MudColor _selectedColor;

        private void Cancel() {
            MudDialog.Cancel();
        }

        protected override void OnParametersSet() {
            if (string.IsNullOrEmpty(ActualColor) == false) {
                var colorSplits = ActualColor.Split(",");
                var r = int.Parse(colorSplits[0]);
                var g = int.Parse(colorSplits[1]);
                var b = int.Parse(colorSplits[2]);

                _selectedColor = new MudColor(r, g, b, 255);
            }

            base.OnParametersSet();
        }

        private void Set() {
            var rgbColor = $"{_selectedColor.R},{_selectedColor.G},{_selectedColor.B}";
            MudDialog.Close(DialogResult.Ok(rgbColor));
        }
    }
}
