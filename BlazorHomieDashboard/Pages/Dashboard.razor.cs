using System.Collections.Generic;
using System.Threading.Tasks;
using DevBot9.Protocols.Homie;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace BlazorHomieDashboard.Pages {
    partial class Dashboard {
        private List<HomieDevice> _homieDevices = new();
        private bool _isLoading = true;
        private string _loadingMessage = "Waiting for server...";

        [Inject]
        private NavigationManager NavigationManager { get; set; }

        private HubConnection _mqttHubConnection;

        protected override async Task OnInitializedAsync() {
            _mqttHubConnection = new HubConnectionBuilder().WithUrl(NavigationManager.ToAbsoluteUri("/HomieHub")).Build();
            _mqttHubConnection.On<string, string>("PublishReceived", HandlePublishReceived);
            _mqttHubConnection.On<List<string>>("CreateDashboard", HandleCreateDashboard);

            await _mqttHubConnection.StartAsync();
            await base.OnInitializedAsync();
        }

        private void PublishToTopic(string topic, string payload, byte qosLevel, bool isRetained) {
            _mqttHubConnection.SendAsync("PublishToTopic", topic, payload, qosLevel, isRetained);
        }

        private void HandleCreateDashboard(List<string> topicsDump) {
            _isLoading = true;

            var newHomieDevices = new List<HomieDevice>();
            var devicesMetadata = HomieTopicTreeParser.Parse(topicsDump.ToArray(), "homie");
            foreach (var deviceMetadata in devicesMetadata) {
                var homieDevice = new HomieDevice();

                homieDevice.Initialize(deviceMetadata, PublishToTopic, (topic => {
                    // No need to subscribe to anything since back-end subscribes all homie topics.
                }));

                newHomieDevices.Add(homieDevice);
            }

            _homieDevices = newHomieDevices;

            if (_homieDevices.Count > 0) {
                _isLoading = false;
            } else {
                _isLoading = true;
                _loadingMessage = "No devices found.";
            }

            StateHasChanged();
        }


        private void HandlePublishReceived(string topic, string payload) {
            foreach (var homieDevice in _homieDevices) {
                homieDevice.HandlePublishReceived(topic, payload);
            }
        }
    }
}