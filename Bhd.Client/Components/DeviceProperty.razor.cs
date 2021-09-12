using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Bhd.Client.Dialogs;
using Bhd.Client.Services;
using Bhd.Client.SignalR;
using Bhd.Shared.DTOs;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using MudBlazor;
using Direction = Bhd.Shared.DTOs.Direction;

namespace Bhd.Client.Components {
    public partial class DeviceProperty : IDisposable {
        [Inject]
        private ILogger<DeviceProperty> Logger { get; set; }

        [Parameter]
        public string PropertyPath { get; set; }

        [Parameter]
        public RenderFragment Buttons { get; set; }

        [Parameter]
        public string AlternativePropertyName { get; set; }

        [Inject]
        public NotificationsHub NotificationsHub { get; set; }

        [Inject]
        public IRestService RestService { get; set; }

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
            var propertyResponse = await RestService.GetAsync<Property>(PropertyPath);
            if (propertyResponse.StatusCode == HttpStatusCode.OK) {
                _property = propertyResponse.Body;
            } else {
                Logger.LogError($"Can't refresh property \"{PropertyPath}\" because API responded with {propertyResponse.StatusCode}");
            }
        }

        public void Dispose() {
            NotificationsHub.DevicePropertyChanged -= HandleDevicePropertyChanged;
        }

        private async Task SetTextValue(string valueToSet) {
            await RestService.PutAsync($"{PropertyPath}/TextValue", valueToSet);
        }

        private async Task SetNumericValue(double valueToSet) {
            await RestService.PutAsync($"{PropertyPath}/NumericValue", valueToSet);
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
            var result = await DialogService.Show<ColorPicker>(_property.Name, dialogParameters).Result;

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
