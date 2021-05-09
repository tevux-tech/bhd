namespace BlazorHomieDashboard.Server.Services {
    interface IMqttBroker {
        public delegate void PublishReceivedDelegate(string topic, string payload);

        event PublishReceivedDelegate PublishReceived;
        void PublishToTopic(string topic, string payload, byte qosLevel, bool isRetained);
        void SubscribeToTopic(string topic);
    }
}