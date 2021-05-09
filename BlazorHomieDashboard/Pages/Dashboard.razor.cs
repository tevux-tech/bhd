using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DevBot9.Protocols.Homie;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace BlazorHomieDashboard.Pages {
    partial class Dashboard {
        private List<HomieDevice> _homieDevices = new();
        private readonly Dictionary<string, string> _topicDump = new();

        private bool _isLoading = true;
        private string _loadingMessage = "";

        [Inject]
        private NavigationManager NavigationManager { get; set; }

        private HubConnection _mqttHubConnection;

        protected override async Task OnInitializedAsync() {
            _mqttHubConnection = new HubConnectionBuilder().WithUrl(NavigationManager.ToAbsoluteUri("/mqttHub")).Build();
            _mqttHubConnection.On<string, string>("PublishReceived", HandlePublishReceived);
            await _mqttHubConnection.StartAsync();

            _loadingMessage = "Server connected. Fetching homie topics...";

            SubscribeToTopic("homie/#");

            var _ = Task.Run(async () => {
                for (var i = 0; i < 10; i++) {
                    _loadingMessage = $"Server connected. Fetching homie topics {_topicDump.Count}...";
                    StateHasChanged();
                    await Task.Delay(100);
                }

                if (_topicDump.Count == 0) {
                    _loadingMessage = "No topics found.";
                    StateHasChanged();
                } else {
                    _loadingMessage = "Creating dashboard...";
                    try {
                        CreateDashboard();
                    } catch (Exception ex) {
                        Console.WriteLine(ex);
                    }

                    _isLoading = false;
                    StateHasChanged();
                }
            });

            await base.OnInitializedAsync();
        }

        private void CreateDashboard() {
            var newHomieDevices = new List<HomieDevice>();

            var localDumpList = new List<string>();
            foreach (var item in _topicDump) {
                localDumpList.Add(item.Key + ":" + item.Value);
            }

            var allDevices = HomieTopicTreeParser.Parse(localDumpList.ToArray(), "homie");
            foreach (var deviceMetadata in allDevices) {
                var homieDevice = new HomieDevice();
                homieDevice.Initialize(deviceMetadata, PublishToTopic, SubscribeToTopic);
                newHomieDevices.Add(homieDevice);
            }

            _homieDevices = newHomieDevices;

            StateHasChanged();
        }


        private void SubscribeToTopic(string topic) {
            _mqttHubConnection.SendAsync("SubscribeToTopic", topic);
        }

        private void PublishToTopic(string topic, string payload, byte qosLevel, bool isRetained) {
            _mqttHubConnection.SendAsync("PublishToTopic", topic, payload, qosLevel, isRetained);
        }


        private void HandlePublishReceived(string topic, string payload) {
            Console.WriteLine($"Handling {topic}={payload}");

            _topicDump[topic] = payload;

            foreach (var homieDevice in _homieDevices) {
                homieDevice.HandlePublishReceived(topic, payload);
            }
        }
    }
}