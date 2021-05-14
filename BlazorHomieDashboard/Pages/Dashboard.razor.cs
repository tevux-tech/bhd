﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DevBot9.Protocols.Homie;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace BlazorHomieDashboard.Pages {
    partial class Dashboard {
        private readonly List<HomieDevice> _homieDevices = new();
        private string _loadingMessage = "Waiting for server...";

        [Inject]
        private NavigationManager NavigationManager { get; set; }

        private HubConnection _mqttHubConnection;

        protected override async Task OnInitializedAsync() {
#if DEBUG
            // This delay is officially recommended by MS.Otherwise breakpoints may not hit.
            await Task.Delay(5000);
#endif

            _mqttHubConnection = new HubConnectionBuilder().WithUrl(NavigationManager.ToAbsoluteUri("/HomieHub")).WithAutomaticReconnect().Build();
            _mqttHubConnection.On<string, string>("PublishReceived", HandlePublishReceived);
            _mqttHubConnection.On<List<string>>("CreateDashboard", HandleCreateDashboard);

            await _mqttHubConnection.StartAsync();
            await base.OnInitializedAsync();
        }

        private void PublishToTopic(string topic, string payload, byte qosLevel, bool isRetained) {
            _mqttHubConnection.SendAsync("PublishToTopic", topic, payload, qosLevel, isRetained);
        }

        private void HandleCreateDashboard(List<string> topicsDump) {
            _homieDevices.Clear();
            _loadingMessage = "Rebuilding...";
            StateHasChanged();

            var devicesMetadata = new HomieTopicTreeParser.Device[0];
            try {
                devicesMetadata = HomieTopicTreeParser.Parse(topicsDump.ToArray(), "homie", out var problemList);

                // Spitting into console, would you Master G. add some logging facilities to BHD?
                Console.WriteLine("Parsed Home dump. If there are any problems, I will print them now:");
                foreach (var problem in problemList) {
                    Console.WriteLine(problem);
                }
                Console.WriteLine("Done.");

            } catch (Exception ex) {
                // Something's wrong with the topic dump.
                // I think this should be logged somewhere?..
                var pzdc = 1;
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

            if (_homieDevices.Count == 0) {
                _loadingMessage = "No devices found. Waiting...";
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
