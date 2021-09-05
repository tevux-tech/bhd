using System;
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

        [Parameter]
        public double Value { get; set; }


        private string _valueString;

        private void Cancel() {
            MudDialog.Cancel();
        }

        protected override void OnParametersSet() {
            _valueString = Value.ToString(CultureInfo.InvariantCulture);
            base.OnParametersSet();
        }

        private void Set() {
            MudDialog.Close(DialogResult.Ok(Value));
        }

        private void Clear() {
            Value = 0;
            _valueString = Value.ToString(CultureInfo.InvariantCulture);
        }

        private void Backspace() {
            if (_valueString.Length == 1) {
                _valueString = "0";
            } else {
                _valueString = _valueString.Remove(_valueString.Length - 1);
            }

            Value = double.Parse(_valueString, CultureInfo.InvariantCulture);
        }

        private void EnterDigit(int digit) {
            _valueString += digit.ToString(CultureInfo.InvariantCulture);
            Value = double.Parse(_valueString, CultureInfo.InvariantCulture);
        }

        private void Comma() {
            if (_valueString.Contains('.') == false) {
                _valueString += ".";
            }

            Value = double.Parse(_valueString, CultureInfo.InvariantCulture);
        }

        private void Negate() {
            if (_valueString.Contains('-')) {
                _valueString = _valueString.Replace("-", "");
            } else {
                _valueString = "-" + _valueString;
            }

            Value = double.Parse(_valueString, CultureInfo.InvariantCulture);
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