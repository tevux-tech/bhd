using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Web;
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

        private List<HomieDevice> _homieDevices = new();
        private readonly Dictionary<string, string> _topicDump = new();

        private void CreateDashboard(MouseEventArgs obj) {
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
            _mqttClient.SubscribeAsync(topic);
        }

        private void Publish(string topic, string payload, byte qoslevel, bool isretained) {
            Console.WriteLine($"Publishing {topic} = {payload}");
            var message = new MqttApplicationMessageBuilder().WithTopic(topic).WithPayload(payload).WithQualityOfServiceLevel(qoslevel).WithRetainFlag(isretained).Build();
            _mqttClient.PublishAsync(message);
        }

        protected override async Task OnInitializedAsync() {
            var factory = new MqttFactory();
            _mqttClient = factory.CreateMqttClient();

            _mqttClient.ApplicationMessageReceivedHandler = new MqttApplicationMessageReceivedHandlerDelegate(HandleMessage);

            var clientOptions = new MqttClientOptions { ChannelOptions = new MqttClientWebSocketOptions { Uri = "ws://192.168.2.2:9001/" } };

            _mqttClient.ConnectedHandler = new MqttClientConnectedHandlerDelegate(async e => {
                Console.WriteLine("### CONNECTED ###");

                await _mqttClient.SubscribeAsync("homie/#");

                Console.WriteLine("### SUBSCRIBED ###");
            });

            _mqttClient.DisconnectedHandler = new MqttClientDisconnectedHandlerDelegate(e => {
                Console.WriteLine("### DISCONNECTED ###");
            });


            try {
                await _mqttClient.ConnectAsync(clientOptions, CancellationToken.None);
            } catch (Exception exception) {
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
