using Microsoft.AspNetCore.Components;
using MudBlazor;
using MudBlazor.Utilities;

namespace Bhd.Client.Dialogs {
    public partial class MyColorPicker {
        [CascadingParameter]
        MudDialogInstance MudDialog { get; set; }

        [Parameter]
        public string ActualColor { get; set; }

        private string _selectedValue;

        private void Cancel() {
            MudDialog.Cancel();
        }

        protected override void OnParametersSet() {
            if (string.IsNullOrEmpty(ActualColor) == false) {
                var colorSplits = ActualColor.Split(",");
                var r = int.Parse(colorSplits[0]);
                var g = int.Parse(colorSplits[1]);
                var b = int.Parse(colorSplits[2]);
                _selectedValue = $"#{r:x2}{g:x2}{b:x2}";
            }

            base.OnParametersSet();
        }

        private void Set() {
            var mudColorParser = new MudColor(_selectedValue);
            var rgbColor = $"{mudColorParser.R},{mudColorParser.G},{mudColorParser.B}";
            MudDialog.Close(DialogResult.Ok(rgbColor));
        }
    }
}
