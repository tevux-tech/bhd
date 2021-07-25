using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using uPLibrary.Networking.M2Mqtt;

namespace Server.Services {
    public class HomieTopicFetcher {
        private MqttClient _mqttClient;
        private readonly string _mqttClientGuid = Guid.NewGuid().ToString();
        private Dictionary<string, string> _responses = new();

        public void Initialize(string mqttBrokerIpAddress) {
            _mqttClient = new MqttClient(mqttBrokerIpAddress);
            _mqttClient.MqttMsgPublishReceived += HandlePublishReceived;
        }

        public void FetchTopics(string filter, out string[] topics) {
            _responses.Clear();
            _mqttClient.Connect(_mqttClientGuid);
            _mqttClient.Subscribe(new[] { filter }, new byte[] { 1 });

            Thread.Sleep(2000);
            _mqttClient.Unsubscribe(new[] { filter });
            _mqttClient.Disconnect();

            topics = new string[_responses.Count];
            var i = 0;
            foreach (var response in _responses) {
                topics[i] = response.Key + ":" + response.Value;
                i++;
            }

            while (_mqttClient.IsConnected) {
                Thread.Sleep(100);
            }
        }

        public void FetchDevices(string baseTopic, out string[] topics) {
            var allTheTopics = new List<string>();

            _mqttClient.Connect(_mqttClientGuid);

            _responses.Clear();
            _mqttClient.Subscribe(new[] { $"{baseTopic}/+/$homie" }, new byte[] { 1 });
            Thread.Sleep(1000);
            _mqttClient.Unsubscribe(new[] { $"{baseTopic}/+/$homie" });

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
                _mqttClient.Subscribe(new[] { $"{baseTopic}/{device}/#" }, new byte[] { 1 });
                Thread.Sleep(100);
                _mqttClient.Unsubscribe(new[] { $"{baseTopic}/{device}/#" });

                Console.WriteLine($"{_responses.Count} topics for {device}.");
                foreach (var topic in _responses) {
                    allTheTopics.Add(topic.Key + ":" + topic.Value);
                }
            }

            _mqttClient.Disconnect();

            topics = allTheTopics.ToArray();

            while (_mqttClient.IsConnected) {
                Thread.Sleep(100);
            }
        }

        private void HandlePublishReceived(object sender, uPLibrary.Networking.M2Mqtt.Messages.MqttMsgPublishEventArgs e) {
            var payload = Encoding.UTF8.GetString(e.Message);

            if (_responses.ContainsKey(e.Topic) == false) {
                _responses.Add(e.Topic, payload);
            } else {
                _responses[e.Topic] = payload;
            }
        }
    }
}