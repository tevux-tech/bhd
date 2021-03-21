using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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

namespace BlazorHomiePlayground.Pages {
    partial class Dashboard {
        private IMqttClient _mqttClient;

        private List<MqttTabData> _tabs = new();
        private readonly MqttObject _rootMqttObject = new();

        private void CreateDashboard(MouseEventArgs obj) {
            var dashboard = new List<MqttTabData>();

            foreach (var mqttObject in _rootMqttObject.Children) {
                dashboard.Add(CreateTab(mqttObject));
            }

            _tabs = dashboard;
            StateHasChanged();
        }

        private MqttTabData CreateTab(MqttObject mqttObject) {
            var tab = new MqttTabData();
            tab.Caption = mqttObject.Name;

            foreach (var nodeName in mqttObject.Nodes) {
                var subTabMqttObject = mqttObject.Children.First(c => c.NodeName == nodeName);
                tab.SubTabs.Add(CreateTab(subTabMqttObject));
            }

            foreach (var propertyName in mqttObject.Properties) {
                var propertyObject = mqttObject.Children.First(c => c.NodeName == propertyName);

                if (propertyObject.Settable == false) {
                    var indicatorData = new MqttIndicatorData();
                    indicatorData.Caption = propertyObject.Name;
                    indicatorData.ActualValue = propertyObject.Value;
                    indicatorData.Units = propertyObject.Unit;

                    propertyObject.ValueChanged += () => {
                        indicatorData.ActualValue = propertyObject.Value;
                        StateHasChanged();
                    };

                    tab.Controls.Add(indicatorData);
                } else {
                    if (propertyObject.Retained) {
                        if (propertyObject.DataType == "float") {
                            var parameterData = new MqttFloatParameterData();
                            parameterData.Caption = propertyObject.Name;
                            parameterData.Units = propertyObject.Unit;
                            parameterData.ActualValue = double.Parse(propertyObject.Value, CultureInfo.InvariantCulture);
                            parameterData.TargetValue = parameterData.ActualValue;

                            propertyObject.ValueChanged += () => {
                                parameterData.ActualValue = double.Parse(propertyObject.Value, CultureInfo.InvariantCulture);
                                parameterData.TargetValue = parameterData.ActualValue;
                                StateHasChanged();
                            };

                            parameterData.SetTargetValue = async () => {
                                var topicToSet = propertyObject.Topic + "/set";
                                var message = new MqttApplicationMessageBuilder().WithTopic(topicToSet).WithPayload(parameterData.TargetValue.ToString(CultureInfo.InvariantCulture)).Build();
                                await _mqttClient.PublishAsync(message);
                            };

                            tab.Controls.Add(parameterData);
                        } else if (propertyObject.DataType == "integer") {
                            var parameterData = new MqttIntegerParameterData();
                            parameterData.Caption = propertyObject.Name;
                            parameterData.Units = propertyObject.Unit;
                            parameterData.ActualValue = int.Parse(propertyObject.Value, CultureInfo.InvariantCulture);
                            parameterData.TargetValue = parameterData.ActualValue;

                            propertyObject.ValueChanged += () => {
                                parameterData.ActualValue = int.Parse(propertyObject.Value, CultureInfo.InvariantCulture);
                                parameterData.TargetValue = parameterData.ActualValue;
                                StateHasChanged();
                            };

                            parameterData.SetTargetValue = async () => {
                                var topicToSet = propertyObject.Topic + "/set";
                                var message = new MqttApplicationMessageBuilder().WithTopic(topicToSet).WithPayload(parameterData.TargetValue.ToString(CultureInfo.InvariantCulture)).Build();
                                await _mqttClient.PublishAsync(message);
                            };

                            tab.Controls.Add(parameterData);
                        } else if (propertyObject.DataType == "color") {
                            var parameterData = new MqttColorParameterData();
                            parameterData.Caption = propertyObject.Name;
                            parameterData.ActualValue = propertyObject.Value;
                            parameterData.TargetValue = parameterData.ActualValue;

                            propertyObject.ValueChanged += () => {
                                parameterData.ActualValue = propertyObject.Value;
                                parameterData.TargetValue = propertyObject.Value;
                                StateHasChanged();
                            };

                            tab.Controls.Add(parameterData);
                        }
                    } else {
                        var commandData = new MqttCommandData();

                        var command1 = "";
                        var command2 = "";

                        if (propertyObject.DataType == "enum") {
                            var enumValues = propertyObject.Format.Split(",");
                            commandData.Button1Caption = enumValues[0];
                            commandData.Button2Caption = enumValues[1];
                            command1 = enumValues[0];
                            command2 = enumValues[1];
                        } else if (propertyObject.DataType == "boolean") {
                            commandData.Button1Caption = "True";
                            commandData.Button2Caption = "False";
                            command1 = "true";
                            command2 = "false";
                        }

                        commandData.ExecuteButton1Action = async () => {
                            var topicToSet = propertyObject.Topic + "/set";
                            var message = new MqttApplicationMessageBuilder().WithTopic(topicToSet).WithPayload(command1).Build();
                            await _mqttClient.PublishAsync(message);
                        };

                        commandData.ExecuteButton2Action = async () => {
                            var topicToSet = propertyObject.Topic + "/set";
                            var message = new MqttApplicationMessageBuilder().WithTopic(topicToSet).WithPayload(command2).Build();
                            await _mqttClient.PublishAsync(message);
                        };

                        commandData.Caption = propertyObject.Name;
                        tab.Controls.Add(commandData);
                    }
                }
            }

            return tab;
        }


        protected override async Task OnInitializedAsync() {
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

            for (int i = 1; i < topicSplits.Length; i++) {
                if (topicSplits[i].StartsWith("$")) break;

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

                    case "$format":
                        objectToUpdate.Format = payload;
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
                objectToUpdate.Topic = topic;
                Console.WriteLine($"{topic}={payload}");

                var previousValue = objectToUpdate.Value;
                objectToUpdate.Value = payload;

                if (previousValue != payload) {
                    objectToUpdate.ValueChanged();
                }
            }
        }
    }
}