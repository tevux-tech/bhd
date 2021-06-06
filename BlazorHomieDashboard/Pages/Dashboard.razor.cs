using System;
using System.Collections.Generic;
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

        private HubConnection _mqttHubConnection;

        private int _topicsCount = 0;

        private string _version;

        private string _sourceCodeUrl;

        protected override async Task OnInitializedAsync() {
            _mqttHubConnection = new HubConnectionBuilder().WithUrl(NavigationManager.ToAbsoluteUri("/HomieHub")).WithAutomaticReconnect(new DashboardReconnectPolicy()).Build();

            _mqttHubConnection.Closed += (exception) => {
                Logger.LogError(exception, "SignalR connection closed.");
                StateHasChanged();
                return Task.FromResult(0);
            };

            _mqttHubConnection.Reconnecting += (exception) => {
                Logger.LogWarning(exception, "SignalR reconnecting");
                StateHasChanged();
                return Task.FromResult(0);
            };

            _mqttHubConnection.Reconnected += (connectionId) => {
                Logger.LogInformation($"Reconnected {connectionId}");
                StateHasChanged();
                return Task.FromResult(0);
            };

            _mqttHubConnection.On<string, string>("PublishReceived", (topic, payload) => {
                try {
                    HandlePublishReceived(topic, payload);
                } catch (Exception ex) {
                    Logger.LogError(ex, $"Processing {topic}={payload} failed.");
                }
            });

            _mqttHubConnection.On<List<string>>("CreateDashboard", (topicDump) => {
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

            await _mqttHubConnection.StartAsync();
            await base.OnInitializedAsync();
        }

        private void PublishToTopic(string topic, string payload, byte qosLevel, bool isRetained) {
            _mqttHubConnection.SendAsync("PublishToTopic", topic, payload, qosLevel, isRetained);
        }

        private void HandleCreateDashboard(List<string> topicsDump) {
            _homieDevices.Clear();
            _topicsCount = topicsDump.Count;

            var devicesMetadata = HomieTopicTreeParser.Parse(topicsDump.ToArray(), "homie", out var parsingErrors);

            foreach (var parsingError in parsingErrors) {
                Logger.LogError(parsingError);
            }

            foreach (var deviceMetadata in devicesMetadata) {
                var homieDevice = DeviceFactory.CreateClientDevice(deviceMetadata);

                homieDevice.Initialize(PublishToTopic, (topic => {
                    // No need to subscribe to anything since back-end subscribes all homie topics.
                }));

                _homieDevices.Add(homieDevice);
            }

            foreach (var dumpValue in topicsDump) {
                var splits = dumpValue.Split(":");

                foreach (var homieDevice in _homieDevices) {
                    homieDevice.HandlePublishReceived(splits[0], splits[1]);
                }
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
