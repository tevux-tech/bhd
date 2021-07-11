﻿using System;
using System.ComponentModel;
using DevBot9.Protocols.Homie;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;

namespace BlazorHomieDashboard {
    public class MqttHubBroker : IClientDeviceConnection {
        private class HubReconnectPolicy : IRetryPolicy {
            public TimeSpan? NextRetryDelay(RetryContext retryContext) {
                return new TimeSpan(0, 0, 5);
            }
        }


        [Inject]
        private ILogger<MqttHubBroker> Logger { get; set; }

        public HubConnection Connection { get; }

        public MqttHubBroker(Uri uri) {
            Connection = new HubConnectionBuilder().WithUrl(uri).WithAutomaticReconnect(new HubReconnectPolicy()).Build();

            Connection.On<string, string>("PublishReceived", (topic, payload) => {
                try {
                    PublishReceived?.Invoke(this, new PublishReceivedEventArgs(topic, payload));
                } catch (Exception ex) {
                    Logger.LogError(ex, $"Processing {topic}={payload} failed.");
                }
            });
        }


        public bool IsConnected { get; } = true;

        public bool TryPublish(string topic, string payload, byte qosLevel, bool isRetained) {
            Connection.SendAsync("PublishToTopic", topic, payload, qosLevel, isRetained);
            return true;
        }

        public bool TrySubscribe(string topic) {
            return true;
        }

        public event PublishReceivedDelegate PublishReceived;

        public virtual void OnPublishReceived(PublishReceivedEventArgs e) {
            PublishReceived?.Invoke(this, e);
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}