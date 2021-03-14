using System;
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

        private string _actualState;

        protected override async Task OnInitializedAsync() {
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
        }

        private void HandleMessage(MqttApplicationMessageReceivedEventArgs obj) {
            if (obj.ApplicationMessage.Topic == "shellies/shelly1pm-68C63AFADFF9/relay/0") {
                var value = Encoding.UTF8.GetString(obj.ApplicationMessage.Payload);
                _actualState = value;
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