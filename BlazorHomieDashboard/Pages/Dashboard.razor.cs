using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Client.Receiving;
using TestApp;

namespace BlazorHomieDashboard.Pages {
    partial class Dashboard {
        private IMqttClient _mqttClient;

        private string _mqttServerUri = "ws://192.168.2.2:9001/";

        private List<HomieDevice> _homieDevices = new();
        private readonly Dictionary<string, string> _topicDump = new();

        private bool _isLoading = true;
        private string _loadingMessage = "";

        private void CreateDashboard() {
            var newHomieDevices = new List<HomieDevice>();

            var localDumpList = new List<string>();
            foreach (var item in _topicDump) {
                localDumpList.Add(item.Key + ":" + item.Value);
            }

            var allDevices = HomieTopicTreeParser.Parse(localDumpList.ToArray(), "homie");
            foreach (var deviceMetadata in allDevices) {
                var homieDevice = new HomieDevice();
                homieDevice.Initialize(deviceMetadata, Publish, Subscribe);
                newHomieDevices.Add(homieDevice);
            }

            _homieDevices = newHomieDevices;

            StateHasChanged();
        }

        private void Subscribe(string topic) {
            Console.WriteLine("Subscribing to " + topic);

            try {
                _mqttClient.SubscribeAsync(topic);
            } catch (Exception ex) {
                Console.WriteLine(ex);
            }
        }

        private void Publish(string topic, string payload, byte qoslevel, bool isretained) {
            Console.WriteLine($"Publishing {topic} = {payload}");

            try {
                var message = new MqttApplicationMessageBuilder().WithTopic(topic).WithPayload(payload).WithQualityOfServiceLevel(qoslevel).WithRetainFlag(isretained).Build();
                _mqttClient.PublishAsync(message);
            } catch (Exception ex) {
                Console.WriteLine(ex);
            }
        }

        protected override async Task OnInitializedAsync() {
            _loadingMessage = $"Connecting to {_mqttServerUri}...";

            var factory = new MqttFactory();
            _mqttClient = factory.CreateMqttClient();

            _mqttClient.ApplicationMessageReceivedHandler = new MqttApplicationMessageReceivedHandlerDelegate(HandleMessage);

            var clientOptions = new MqttClientOptions { ChannelOptions = new MqttClientWebSocketOptions { Uri = _mqttServerUri } };

            _mqttClient.ConnectedHandler = new MqttClientConnectedHandlerDelegate(async e => {
                _loadingMessage = "Server connected. Fetching homie topics...";

                await _mqttClient.SubscribeAsync("homie/#");

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
                        CreateDashboard();
                        _isLoading = false;
                        StateHasChanged();
                    }
                });
            });

            _mqttClient.DisconnectedHandler = new MqttClientDisconnectedHandlerDelegate(e => {
                _isLoading = true;
                _loadingMessage = "Server disconnected";
                StateHasChanged();
                Console.WriteLine("### DISCONNECTED ###");
            });


            try {
                await _mqttClient.ConnectAsync(clientOptions, CancellationToken.None);
            } catch (Exception exception) {
                _loadingMessage = $"Failed connecting to {_mqttServerUri}";
                Console.WriteLine("### CONNECTING FAILED ###" + Environment.NewLine + exception);
            }

            await base.OnInitializedAsync();
        }


        private void HandleMessage(MqttApplicationMessageReceivedEventArgs obj) {
            var payload = Encoding.UTF8.GetString(obj.ApplicationMessage.Payload);
            var topic = obj.ApplicationMessage.Topic;

            Console.WriteLine($"Handling {topic}={payload}");

            _topicDump[topic] = payload;

            foreach (var homieDevice in _homieDevices) {
                homieDevice.HandlePublishReceived(topic, payload);
            }
        }
    }
}