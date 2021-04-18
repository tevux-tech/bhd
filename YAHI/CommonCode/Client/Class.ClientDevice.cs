using System.Collections;

namespace DevBot9.Protocols.Homie {
    /// <summary>
    /// This is a Client Device implementation. It should be used to consume a Homie Device that is already present on the MQTT broker.
    /// </summary>
    public class ClientDevice : Device {
        #region Public interface

        /// <summary>
        /// Initializes the entire Client Device tree: actually creates internal property variables, subscribes to topics and so on. This method must be called, or otherwise entire Client Device tree will not work.
        /// </summary>
        /// <param name="publishToTopicDelegate">This is a mandatory publishing delegate. Wihout it, Client Device will not work.</param>
        /// <param name="subscribeToTopicDelegate">This is a mandatory subscription delegate. Wihout it, Client Device will not work.</param>
        public new void Initialize(PublishToTopicDelegate publishToTopicDelegate, SubscribeToTopicDelegate subscribeToTopicDelegate) {
            base.Initialize(publishToTopicDelegate, subscribeToTopicDelegate);

            var homieTopic = $"{_baseTopic}/{_deviceId}/$homie";
            _topicHandlerMap.Add(homieTopic, new ArrayList());
            ActionString handler = delegate (string value) { HomieVersion = value; };
            ((ArrayList)_topicHandlerMap[homieTopic]).Add(handler);
            _subscribeToTopicDelegate(homieTopic);

            var nameTopic = $"{_baseTopic}/{_deviceId}/$name";
            _topicHandlerMap.Add(nameTopic, new ArrayList());
            ActionString handler2 = delegate (string value) { Name = value; };
            ((ArrayList)_topicHandlerMap[nameTopic]).Add(handler);
            _subscribeToTopicDelegate(nameTopic);

            var stateTopic = $"{_baseTopic}/{_deviceId}/$state";
            _topicHandlerMap.Add(stateTopic, new ArrayList());
            ActionString handler3 = delegate (string value) {
                // Payload is lower case, so we need to do some magic štuff to validate it.
                if (value.Length >= 4) {
                    var uppercasedPayload = value.Substring(0, 1).ToUpper() + value.Substring(1);
#warning Need to port to nF
                    //if (Enum.TryParse<HomieState>(uppercasedPayload, out var parsedState)) {
                    //    State = parsedState;
                    //};
                }
            };
            ((ArrayList)_topicHandlerMap[stateTopic]).Add(handler);
            _subscribeToTopicDelegate(stateTopic);
        }

        /// <summary>
        /// Creates a client string property.
        /// </summary>
        public ClientStringProperty CreateClientStringProperty(ClientPropertyMetadata creationOptions) {
            var createdProperty = new ClientStringProperty(creationOptions);

            _properties.Add(createdProperty);

            return createdProperty;
        }

        /// <summary>
        /// Creates a client integer property.
        /// </summary>
        public ClientIntegerProperty CreateClientIntegerProperty(ClientPropertyMetadata creationOptions) {
            var createdProperty = new ClientIntegerProperty(creationOptions);

            _properties.Add(createdProperty);

            return createdProperty;
        }

        /// <summary>
        /// Creates a client float property.
        /// </summary>
        public ClientFloatProperty CreateClientFloatProperty(ClientPropertyMetadata creationOptions) {
            var createdProperty = new ClientFloatProperty(creationOptions);

            _properties.Add(createdProperty);

            return createdProperty;
        }

        /// <summary>
        /// Creates a client boolean property.
        /// </summary>
        public ClientBooleanProperty CreateClientBooleanProperty(ClientPropertyMetadata creationOptions) {
            var createdProperty = new ClientBooleanProperty(creationOptions);

            _properties.Add(createdProperty);

            return createdProperty;
        }
        #endregion

        #region Private stuff

        internal ClientDevice(string baseTopic, string id) {
            _baseTopic = baseTopic;
            _deviceId = id;
        }

        #endregion
    }
}
