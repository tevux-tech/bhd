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

namespace BlazorHomiePlayground.Pages {
    partial class Dashboard {
        private IMqttClient _mqttClient;


        private List<MqttTabData> _tabs = new List<MqttTabData>();

        private MqttIndicatorData _someIndicator;

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

                await _mqttClient.SubscribeAsync("shellies/shelly1pm-68C63AFADFF9/relay/0");

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

            StateHasChanged();

            await Task.Run(async () => {
                var i = 0;
                while (true) {
                    _someIndicator.Value = i.ToString();
                    _someIndicator.OnStateChanged();
                    i++;
                    await Task.Delay(200);

                    //StateHasChanged();
                }
            });
        }

        private void HandleMessage(MqttApplicationMessageReceivedEventArgs obj) {
            if (obj.ApplicationMessage.Topic == "shellies/shelly1pm-68C63AFADFF9/relay/0") {
                var value = Encoding.UTF8.GetString(obj.ApplicationMessage.Payload);
                //_actualState = value;
                Console.WriteLine(value);

                StateHasChanged();
            }
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