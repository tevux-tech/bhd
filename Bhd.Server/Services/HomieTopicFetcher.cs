using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Tevux.Protocols.Mqtt;

namespace Bhd.Server.Services {
    public class HomieTopicFetcher {
        private MqttClient _mqttClient = new MqttClient();
        private Dictionary<string, string> _responses = new();
        private ChannelConnectionOptions _channelConnectionOptions;


        public void Initialize(ChannelConnectionOptions channelOptions) {
            _channelConnectionOptions = channelOptions;
            _mqttClient.Initialize();

            _mqttClient.PublishReceived += HandlePublishReceived;
        }

        public void FetchTopics(string filter, out string[] topics) {
            _responses.Clear();
            _mqttClient.Connect(_channelConnectionOptions);
            while (_mqttClient.IsConnected == false) {
                Thread.Sleep(100);
            }

            _mqttClient.Subscribe(filter, QosLevel.AtLeastOnce);

            Thread.Sleep(5000);
            //_mqttClient.Unsubscribe(filter);
            //_mqttClient.Disconnect();

            topics = new string[_responses.Count];
            var i = 0;
            foreach (var response in _responses) {
                topics[i] = response.Key + ":" + response.Value;
                i++;
            }

            //while (_mqttClient.IsConnected) { Thread.Sleep(100); }
        }

        public void FetchDevices(string baseTopic, out string[] topics) {
            var allTheTopics = new List<string>();

            _mqttClient.Connect(_channelConnectionOptions);
            while (_mqttClient.IsConnected == false) {
                Thread.Sleep(100);
            }

            _responses.Clear();
            _mqttClient.Subscribe($"{baseTopic}/+/$homie", QosLevel.AtLeastOnce);
            Thread.Sleep(1000);
            _mqttClient.Unsubscribe($"{baseTopic}/+/$homie");

            Console.WriteLine($"Found {_responses.Count} homie devices.");
            var devices = new List<string>();
            foreach (var deviceTopic in _responses) {
                var deviceName = deviceTopic.Key.Split('/')[1];
                devices.Add(deviceName);
                Console.Write(deviceName + " ");
            }

            Console.WriteLine();

            foreach (var device in devices) {
                _responses.Clear();
                _mqttClient.Subscribe($"{baseTopic}/{device}/#", QosLevel.AtLeastOnce);
                Thread.Sleep(100);
                _mqttClient.Unsubscribe($"{baseTopic}/{device}/#");

                Console.WriteLine($"{_responses.Count} topics for {device}.");
                foreach (var topic in _responses) {
                    allTheTopics.Add(topic.Key + ":" + topic.Value);
                }
            }

            _mqttClient.Disconnect();

            topics = allTheTopics.ToArray();

            while (_mqttClient.IsConnected) { Thread.Sleep(100); }
        }

        private void HandlePublishReceived(object sender, PublishReceivedEventArgs e) {
            var payload = Encoding.UTF8.GetString(e.Message);

            if (_responses.ContainsKey(e.Topic) == false) { _responses.Add(e.Topic, payload); } else { _responses[e.Topic] = payload; }
        }
    }
}