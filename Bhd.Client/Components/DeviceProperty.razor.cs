using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Bhd.Client.Dialogs;
using Bhd.Shared.DTOs;
using Microsoft.AspNetCore.Components;
using MudBlazor;
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
            var result = await DialogService.Show<ColorPicker>(null, dialogParameters).Result;

            if (result.Cancelled == false) {
                await SetTextValue(result.Data.ToString());
            }
        }

        private async Task EditNumber() {
            var dialogParameters = new DialogParameters();
            dialogParameters["Unit"] = _property.Unit;
            dialogParameters["Value"] = _property.NumericValue;

            var result = await DialogService.Show<NumberPicker>(_property.Name, dialogParameters).Result;

            if (result.Cancelled == false) {
                await SetNumericValue((double)result.Data);
            }
        }

        private string GetColorIndicatorBackgroundStyle() {
            return $"background-color: rgb({_property.TextValue})";
        }
    }
}
