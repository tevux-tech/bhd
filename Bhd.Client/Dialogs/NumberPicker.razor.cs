using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;

namespace Bhd.Client.Dialogs {
    public partial class NumberPicker {
        [CascadingParameter]
        MudDialogInstance MudDialog { get; set; }

        [Parameter]
        public string Unit { get; set; }

        private string _valueString;

        [Parameter]
        public double Value {
            get {
                if (double.TryParse(_valueString, out var result)) {
                    return result;
                }

                return 0;
            }

            set {
                _valueString = value.ToString(CultureInfo.InvariantCulture);
            }
        }


        private void Cancel() {
            MudDialog.Cancel();
        }

        private void Set() {
            MudDialog.Close(DialogResult.Ok(Value));
        }

        private void Clear() {
            _valueString = "0";
        }

        private void Backspace() {
            if (_valueString.Length == 1) {
                _valueString = "0";
            } else {
                _valueString = _valueString.Remove(_valueString.Length - 1);
            }
        }

        private void EnterDigit(int digit) {
            _valueString += digit.ToString(CultureInfo.InvariantCulture);
        }

        private void EnterComma() {
            if (_valueString.Contains('.') == false) {
                _valueString += ".";
            }
        }

        private void Negate() {
            if (_valueString.Contains('-')) {
                _valueString = _valueString.Replace("-", "");
            } else {
                _valueString = "-" + _valueString;
            }
        }

        public void HandleKeyDown(KeyboardEventArgs obj) {
            if (obj.Key == "Enter") {
                Task.Run(() => {
                    MudDialog.Close(DialogResult.Ok(Value));
                });
            }
        }
    }
}