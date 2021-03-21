using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Client.Receiving;

namespace BlazorHomiePlayground.Pages {
    partial class Dashboard {
        private IMqttClient _mqttClient;


        private List<MqttTabData> _tabs = new List<MqttTabData>();

        private MqttIndicatorData _someIndicator;

        private MqttObject _rootMqttObject = new MqttObject();

        private MqttTabData CreateAirConditioningTab() {
            var tab = new MqttTabData();
            tab.Caption = "Air conditioning unit";

            var subtab1 = new MqttTabData() { Caption = "General information and properties" };
            subtab1.Controls.Add(_someIndicator);
            subtab1.Controls.Add(new MqttIndicatorData { Caption = "Actual power state", Value = "OFF" });
            subtab1.Controls.Add(new MqttCommandData { Caption = "On/off switch" });
            subtab1.Controls.Add(new MqttNudData { Caption = "Target air temperature", ActualValue = 24, Units = "°C" });

            var subtab2 = new MqttTabData { Caption = "Ventilation information and properties" };
            var subtab3 = new MqttTabData { Caption = "Service related properties" };

            tab.SubTabs.Add(subtab1);
            tab.SubTabs.Add(subtab2);
            tab.SubTabs.Add(subtab3);
            return tab;
        }

        private MqttTabData CreatePedroTab() {
            var tab = new MqttTabData();
            tab.Caption = "Bybis";

            var subtab1 = new MqttTabData() { Caption = "General x information and properties" };
            subtab1.Controls.Add(new MqttNudData { Caption = "Target air temperature", ActualValue = 24, Units = "°C" });

            var subtab2 = new MqttTabData { Caption = "Ventilation x information and properties" };
            var subtab3 = new MqttTabData { Caption = "Service related properties" };

            tab.SubTabs.Add(subtab1);
            tab.SubTabs.Add(subtab2);
            tab.SubTabs.Add(subtab3);
            return tab;
        }

        protected override async Task OnInitializedAsync() {
            _someIndicator = new MqttIndicatorData { Caption = "Actual measured air temperature", Value = "23.9 °C" };

            _tabs.Add(CreateAirConditioningTab());
            _tabs.Add(CreatePedroTab());

            var factory = new MqttFactory();
            _mqttClient = factory.CreateMqttClient();

            _mqttClient.ApplicationMessageReceivedHandler = new MqttApplicationMessageReceivedHandlerDelegate(HandleMessage);

            var clientOptions = new MqttClientOptions { ChannelOptions = new MqttClientWebSocketOptions { Uri = "ws://192.168.2.2:9001/" } };

            _mqttClient.ConnectedHandler = new MqttClientConnectedHandlerDelegate(async e => {
                Console.WriteLine("### CONNECTED ###");

                await _mqttClient.SubscribeAsync("homie/#");
                //await _mqttClient.SubscribeAsync("homie/shelly1pm-68C63AFADFF9/relay/0");

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

            var topicSplits = topic.Split('/');

            var objectToUpdate = _rootMqttObject;

            for (int i = 1; i < topicSplits.Length - 1; i++) {
                var childToUpdate = objectToUpdate.Children.FirstOrDefault(c => c.NodeName == topicSplits[i]);

                if (childToUpdate == null) {
                    childToUpdate = new MqttObject();
                    childToUpdate.NodeName = topicSplits[i];
                    objectToUpdate.Children.Add(childToUpdate);
                }

                objectToUpdate = childToUpdate;
            }

            if (topicSplits.Last().StartsWith("$")) {
                switch (topicSplits.Last()) {
                    case "$name":
                        objectToUpdate.Name = payload;
                        break;

                    case "$datatype":
                        objectToUpdate.DataType = payload;
                        break;

                    case "$settable":
                        objectToUpdate.Settable = payload == "true";
                        break;

                    case "$retained":
                        objectToUpdate.Retained = payload == "true";
                        break;

                    case "$unit":
                        objectToUpdate.Unit = payload;
                        break;

                    case "$homie":
                        objectToUpdate.Homie = payload;
                        break;

                    case "$type":
                        objectToUpdate.Type = payload;
                        break;

                    case "$state":
                        objectToUpdate.State = payload;
                        break;

                    case "$nodes":
                        objectToUpdate.Nodes = payload.Split(',').ToList();
                        break;

                    case "$properties":
                        objectToUpdate.Properties = payload.Split(',').ToList();
                        break;
                }
            } else {
                objectToUpdate.Value = payload;
            }

            Console.WriteLine($"{topic} = {payload}");
        }

        private async Task TurnOn() {
            var message = new MqttApplicationMessageBuilder().WithTopic("shellies/shelly1pm-68C63AFADFF9/relay/0/command").WithPayload("on").Build();

            await _mqttClient.PublishAsync(message);
        }

        private async Task TurnOff() {
            var message = new MqttApplicationMessageBuilder().WithTopic("shellies/shelly1pm-68C63AFADFF9/relay/0/command").WithPayload("off").Build();

            await _mqttClient.PublishAsync(message);
        }
    }
}