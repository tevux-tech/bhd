using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;

namespace BlazorHomieDashboard.Pages {
    partial class Dashboard {
        private readonly List<HomieDevice> _homieDevices = new();

        [Inject]
        private NavigationManager NavigationManager { get; set; }

        [Inject]
        private ILogger<Dashboard> Logger { get; set; }

        [Inject]
        private HttpClient HttpClient { get; set; }

        private HubConnection _mqttHubConnection;

        private int _topicsCount = 0;

        private string _version;

        protected override async Task OnInitializedAsync() {
            _mqttHubConnection = new HubConnectionBuilder().WithUrl(NavigationManager.ToAbsoluteUri("/HomieHub")).WithAutomaticReconnect().Build();

            _mqttHubConnection.Closed += async (exception) => {
                Logger.LogError(exception, "SignalR connection closed.");
                StateHasChanged();
            };

            _mqttHubConnection.Reconnecting += async (exception) => {
                Logger.LogWarning(exception, "SignalR reconnecting");
                StateHasChanged();
            };

            _mqttHubConnection.Reconnected += async (connectionId) => {
                Logger.LogInformation($"Reconnected {connectionId}");
                StateHasChanged();
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
                var homieDevice = new HomieDevice();

                homieDevice.Initialize(deviceMetadata, PublishToTopic, (topic => {
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