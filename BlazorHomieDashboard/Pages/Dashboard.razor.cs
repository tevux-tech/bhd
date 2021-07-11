using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using DevBot9.Protocols.Homie;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;

namespace BlazorHomieDashboard.Pages {
    partial class Dashboard {
        private class DashboardReconnectPolicy : IRetryPolicy {
            public TimeSpan? NextRetryDelay(RetryContext retryContext) {
                return new TimeSpan(0, 0, 5);
            }
        }

        private readonly List<ClientDevice> _homieDevices = new();

        [Inject]
        private NavigationManager NavigationManager { get; set; }

        [Inject]
        private ILogger<Dashboard> Logger { get; set; }

        [Inject]
        private HttpClient HttpClient { get; set; }

        private MqttHubBroker _mqttHubBroker;

        private int _topicsCount = 0;

        private string _version;

        private string _sourceCodeUrl;

        protected override async Task OnInitializedAsync() {
            _mqttHubBroker = new MqttHubBroker(NavigationManager.ToAbsoluteUri("/HomieHub"));

            _mqttHubBroker.Connection.Closed += (exception) => {
                Logger.LogError(exception, "SignalR connection closed.");
                StateHasChanged();
                return Task.FromResult(0);
            };

            _mqttHubBroker.Connection.Reconnecting += (exception) => {
                Logger.LogWarning(exception, "SignalR reconnecting");
                StateHasChanged();
                return Task.FromResult(0);
            };

            _mqttHubBroker.Connection.Reconnected += (connectionId) => {
                Logger.LogInformation($"Reconnected {connectionId}");
                StateHasChanged();
                return Task.FromResult(0);
            };

            _mqttHubBroker.Connection.On<List<KeyValuePair<string, string>>>("CreateDashboard", (topicDump) => {
                try {
                    HandleCreateDashboard(topicDump);
                } catch (Exception ex) {
                    Logger.LogError(ex, "Creating dashboard failed.");
                }
            });

            try {
                _version = await HttpClient.GetFromJsonAsync<string>("Version");
            } catch (Exception ex) {
                Logger.LogError(ex, "Unable to read version.");
            }

            try {
                _sourceCodeUrl = await HttpClient.GetFromJsonAsync<string>("Version/SourceCodeUrl");
            } catch (Exception ex) {
                Logger.LogError(ex, "Unable to read source code url.");
            }

            await _mqttHubBroker.Connection.StartAsync();
            await base.OnInitializedAsync();
        }

        private void HandleCreateDashboard(List<KeyValuePair<string, string>> topicsDump) {
            foreach (var homieDevice in _homieDevices) {
                homieDevice.Dispose();
            }

            _homieDevices.Clear();
            StateHasChanged();

            _topicsCount = topicsDump.Count;

            var devicesMetadata = HomieTopicTreeParser.Parse(topicsDump.Select(d => d.Key + ":" + d.Value).ToArray(), "homie", out var parsingErrors);

            foreach (var parsingError in parsingErrors) {
                Logger.LogError(parsingError);
            }

            foreach (var deviceMetadata in devicesMetadata) {
                var homieDevice = DeviceFactory.CreateClientDevice(deviceMetadata);
                homieDevice.Initialize(_mqttHubBroker);
                _homieDevices.Add(homieDevice);
            }

            foreach (var dumpValue in topicsDump) {
                _mqttHubBroker.OnPublishReceived(new PublishReceivedEventArgs(dumpValue.Key, dumpValue.Value));
            }

            StateHasChanged();
        }
    }
}