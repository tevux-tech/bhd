using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Bhd.Client.Dialogs;
using Bhd.Shared.DTOs;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using MudBlazor.Utilities;
using Direction = Bhd.Shared.DTOs.Direction;

namespace Bhd.Client.Components {
    public partial class DeviceProperty : IDisposable {

        [Parameter]
        public string PropertyPath { get; set; }

        [Parameter]
        public RenderFragment Buttons { get; set; }

        [Parameter]
        public string AlternativePropertyName { get; set; }

        [Inject]
        public NotificationsHub NotificationsHub { get; set; }

        [Inject]
        public HttpClient HttpClient { get; set; }

        [Inject]
        private IDialogService DialogService { get; set; }

        public MudNumericField<double> _targetNumericField;

        private bool _isEditing;
        private double _targetNumericValue;

        private Property _property = new();

        protected override Task OnInitializedAsync() {
            NotificationsHub.DevicePropertyChanged += HandleDevicePropertyChanged;
            return base.OnInitializedAsync();
        }

        private void HandleDevicePropertyChanged(string propertyPath) {
            if (propertyPath == PropertyPath) {
                Task.Run(async () => {
                    await Refresh();
                    StateHasChanged();
                });
            }
        }

        protected override async Task OnParametersSetAsync() {
            _isEditing = false;
            await Refresh();
        }


        private async Task Refresh() {
            _property = await HttpClient.GetFromJsonAsync<Property>(PropertyPath);
        }

        public void Dispose() {
            NotificationsHub.DevicePropertyChanged -= HandleDevicePropertyChanged;
        }

        private async Task SetTextValue(string valueToSet) {
            await HttpClient.PutAsJsonAsync($"{PropertyPath}/TextValue", valueToSet);
        }

        private async Task SetNumericValue(double valueToSet) {
            await HttpClient.PutAsJsonAsync($"{PropertyPath}/NumericValue", valueToSet);
        }

        private void CancelEdit() {
            _isEditing = false;
        }

        private Color GetChoiceColor(string choice) {
            if (_property.Direction == Direction.Write) {
                return Color.Default;
            }

            if (_property.TextValue == choice) {
               return Color.Primary;
            }

            return Color.Default;
        }


        private async Task EditColor() {
            var dialogParameters = new DialogParameters();
            dialogParameters["ActualColor"] = _property.TextValue;
            var result = await DialogService.Show<MyColorPicker>(null, dialogParameters).Result;

            if (result.Cancelled == false) {
                await SetTextValue(result.Data.ToString());
            }
        }

        private void Edit() {
            _targetNumericValue = _property.NumericValue;
            _isEditing = true;

            // Selecting text in the control after some time. Doesn't work if control is not yet visible ant it takes some time for visibilities to update.
            Task.Run(async () => {
                await Task.Delay(200);
                await _targetNumericField.SelectAsync();
            });
        }

        private string GetColorIndicatorBackgroundStyle() {
            return $"background-color: rgb({_property.TextValue})";
        }

        private async Task HandleSetButtonClick(MouseEventArgs obj) {
            await SetNumericValue(_targetNumericValue);
            _isEditing = false;
        }

        private async Task HandleNudKeyPress(KeyboardEventArgs obj) {
            if (obj.Key == "Enter") {
                await SetNumericValue(_targetNumericValue);
                _isEditing = false;
            }
        }

    }
}
