using System;
using System.Collections;
using System.Threading;

namespace DevBot9.Protocols.Homie {
    /// <summary>
    /// This is a Host Device implementation. It should be used to create a Homie Device that is not yet actually present on the MQTT broker.
    /// </summary>
    public class HostDevice : Device {
        #region Public interface

        /// <summary>
        /// Initializes the entire Host Device tree: actually creates internal property variables, publishes to topics and so on. This method must be called, or otherwise entire Host Device tree will not work.
        /// </summary>
        /// <param name="publishToTopicDelegate">This is a mandatory publishing delegate. Wihout it, Host Device will not work.</param>
        /// <param name="subscribeToTopicDelegate">This is a mandatory subscription delegate. Wihout it, Host Device will not work.</param>
        public new void Initialize(PublishToTopicDelegate publishToTopicDelegate, SubscribeToTopicDelegate subscribeToTopicDelegate) {
            // This will initialize all the properties.
            base.Initialize(publishToTopicDelegate, subscribeToTopicDelegate);

            // One can pretty much do anything while in "init" state.
            SetState(HomieState.Init);

            // Building node subtree.
            var nodesList = "";
            foreach (NodeInfo node in _nodes) {
                InternalGeneralPublish($"{_baseTopic}/{_deviceId}/{node.Id}/$name", node.Name);
                InternalGeneralPublish($"{_baseTopic}/{_deviceId}/{node.Id}/$type", node.Type);
                InternalGeneralPublish($"{_baseTopic}/{_deviceId}/{node.Id}/$properties", node.Properties);

                nodesList += "," + node.Id;
            }
            nodesList = nodesList.Substring(1, nodesList.Length - 1);

            // The order in which these master properties are published may be important for the property discovery implementation.
            // I have a feeling that OpenHAB, HomeAssistant and other do actually behave differently if I rearrange properties below,
            // but the exact order is not specified in the convention...
            InternalGeneralPublish($"{_baseTopic}/{_deviceId}/$homie", HomieVersion);
            InternalGeneralPublish($"{_baseTopic}/{_deviceId}/$name", Name);
            InternalGeneralPublish($"{_baseTopic}/{_deviceId}/$nodes", nodesList);

            // imitating some initialization work.
            Thread.Sleep(1000);

            // Off we go. At this point discovery services should rebuild their trees.
            SetState(HomieState.Ready);
        }

        /// <summary>
        /// Set the state of the Device. Homie convention defines these states: init, ready, disconnected, sleeping, lost, alert.
        /// </summary>
        public void SetState(HomieState stateToSet) {
            State = stateToSet;
            InternalGeneralPublish($"{_baseTopic}/{_deviceId}/$state", State.ToHomiePayload());
        }

        /// <summary>
        /// Creates and/or updates the node for the Device. Nodes are somewhat virtual in Homie, you can create them at any time when build your Device tree, as long as <see cref="Initialize(PublishToTopicDelegate, SubscribeToTopicDelegate)"/> is not called yet.
        /// </summary>
        public void UpdateNodeInfo(string nodeId, string friendlyName, string type) {
            if (DeviceFactory.ValidateTopicLevel(nodeId, out var validationMessage) == false) { throw new ArgumentException(validationMessage, nameof(nodeId)); }

            // Trying to find if that's an existing node.
            NodeInfo nodeToUpdate = null;
            for (var i = 0; i < _nodes.Count; i++) {
                var currentNode = (NodeInfo)_nodes[i];
                if (currentNode.Id == nodeId) { nodeToUpdate = currentNode; }
            }

            // If not found - add new. Otherwise - update.
            if (nodeToUpdate == null) {
                nodeToUpdate = new NodeInfo() { Id = nodeId, Name = friendlyName, Type = type };
                _nodes.Add(nodeToUpdate);
            }
            else {
                nodeToUpdate.Name = friendlyName;
                nodeToUpdate.Type = type;
            }
        }

        /// <summary>
        /// Creates a host integer property.
        /// </summary>
        public HostIntegerProperty CreateHostIntegerProperty(PropertyType propertyType, string nodeId, string propertyId, string friendlyName, int initialValue = 0, string unit = "") {
            if (DeviceFactory.ValidateTopicLevel(nodeId, out var validationMessage) == false) { throw new ArgumentException(validationMessage, nameof(nodeId)); }
            if (DeviceFactory.ValidateTopicLevel(propertyId, out var validationMessage2) == false) { throw new ArgumentException(validationMessage2, nameof(nodeId)); }

            UpdateNodePropertyMap(nodeId, propertyId);

            var createdProperty = new HostIntegerProperty(propertyType, $"{nodeId}/{propertyId}", friendlyName, initialValue, "", unit);

            _properties.Add(createdProperty);

            return createdProperty;
        }

        /// <summary>
        /// Creates a host float property.
        /// </summary>
        public HostFloatProperty CreateHostFloatProperty(PropertyType propertyType, string nodeId, string propertyId, string friendlyName, float initialValue = 0.0f, string unit = "") {
            if (DeviceFactory.ValidateTopicLevel(nodeId, out var validationMessage) == false) { throw new ArgumentException(validationMessage, nameof(nodeId)); }
            if (DeviceFactory.ValidateTopicLevel(propertyId, out var validationMessage2) == false) { throw new ArgumentException(validationMessage2, nameof(nodeId)); }

            UpdateNodePropertyMap(nodeId, propertyId);

            var createdProperty = new HostFloatProperty(propertyType, $"{nodeId}/{propertyId}", friendlyName, initialValue, "", unit);

            _properties.Add(createdProperty);

            return createdProperty;
        }

        /// <summary>
        /// Creates a host string property.
        /// </summary>
        public HostStringProperty CreateHostStringProperty(PropertyType propertyType, string nodeId, string propertyId, string friendlyName, string initialValue = "", string unit = "") {
            if (DeviceFactory.ValidateTopicLevel(nodeId, out var validationMessage) == false) { throw new ArgumentException(validationMessage, nameof(nodeId)); }
            if (DeviceFactory.ValidateTopicLevel(propertyId, out var validationMessage2) == false) { throw new ArgumentException(validationMessage2, nameof(nodeId)); }

            UpdateNodePropertyMap(nodeId, propertyId);

            var createdProperty = new HostStringProperty(propertyType, $"{nodeId}/{propertyId}", friendlyName, initialValue, "", unit);

            _properties.Add(createdProperty);

            return createdProperty;
        }

        /// <summary>
        /// Creates a host boolean property.
        /// </summary>
        public HostBooleanProperty CreateHostBooleanProperty(PropertyType propertyType, string nodeId, string propertyId, string friendlyName, bool initialValue = false) {
            if (DeviceFactory.ValidateTopicLevel(nodeId, out var validationMessage) == false) { throw new ArgumentException(validationMessage, nameof(nodeId)); }
            if (DeviceFactory.ValidateTopicLevel(propertyId, out var validationMessage2) == false) { throw new ArgumentException(validationMessage2, nameof(nodeId)); }

            UpdateNodePropertyMap(nodeId, propertyId);

            var createdProperty = new HostBooleanProperty(propertyType, $"{nodeId}/{propertyId}", friendlyName, initialValue, "", "");

            _properties.Add(createdProperty);

            return createdProperty;
        }

        /// <summary>
        /// Creates a host color property.
        /// </summary>
        public HostColorProperty CreateHostColorProperty(PropertyType propertyType, string nodeId, string propertyId, string friendlyName, ColorFormat colorFormat = ColorFormat.Rgb) {
            if (DeviceFactory.ValidateTopicLevel(nodeId, out var validationMessage) == false) { throw new ArgumentException(validationMessage, nameof(nodeId)); }
            if (DeviceFactory.ValidateTopicLevel(propertyId, out var validationMessage2) == false) { throw new ArgumentException(validationMessage2, nameof(nodeId)); }

            UpdateNodePropertyMap(nodeId, propertyId);

            var createdProperty = new HostColorProperty(propertyType, $"{nodeId}/{propertyId}", friendlyName, colorFormat, "");

            _properties.Add(createdProperty);

            return createdProperty;
        }

        /// <summary>
        /// Creates a host enum property.
        /// </summary>
        public HostEnumProperty CreateHostEnumProperty(PropertyType propertyType, string nodeId, string propertyId, string friendlyName, in string[] possibleValues, string initialValue = "") {
            if (DeviceFactory.ValidateTopicLevel(nodeId, out var validationMessage) == false) { throw new ArgumentException(validationMessage, nameof(nodeId)); }
            if (DeviceFactory.ValidateTopicLevel(propertyId, out var validationMessage2) == false) { throw new ArgumentException(validationMessage2, nameof(nodeId)); }

            UpdateNodePropertyMap(nodeId, propertyId);

            var createdProperty = new HostEnumProperty(propertyType, $"{nodeId}/{propertyId}", friendlyName, possibleValues, initialValue);

            _properties.Add(createdProperty);

            return createdProperty;
        }

        /// <summary>
        /// Creates a host date ant time property.
        /// </summary>
        public HostDateTimeProperty CreateHostDateTimeProperty(PropertyType propertyType, string nodeId, string propertyId, string friendlyName, DateTime initialValue) {
            if (DeviceFactory.ValidateTopicLevel(nodeId, out var validationMessage) == false) { throw new ArgumentException(validationMessage, nameof(nodeId)); }
            if (DeviceFactory.ValidateTopicLevel(propertyId, out var validationMessage2) == false) { throw new ArgumentException(validationMessage2, nameof(nodeId)); }

            UpdateNodePropertyMap(nodeId, propertyId);

            var createdProperty = new HostDateTimeProperty(propertyType, $"{nodeId}/{propertyId}", friendlyName, initialValue);

            _properties.Add(createdProperty);

            return createdProperty;
        }

        #endregion

        #region Private stuff

        private readonly ArrayList _nodes = new ArrayList();

        internal HostDevice(string baseTopic, string id, string friendlyName = "") {
            _baseTopic = baseTopic;
            _deviceId = id;
            Name = friendlyName;
            State = HomieState.Init;
        }


        private void UpdateNodePropertyMap(string nodeId, string propertyId) {
            // Trying to find if that's an existing node.
            NodeInfo nodeToUpdate = null;
            for (var i = 0; i < _nodes.Count; i++) {
                var currentNode = (NodeInfo)_nodes[i];
                if (currentNode.Id == nodeId) { nodeToUpdate = currentNode; }
            }

            // If not found - add new.
            if (nodeToUpdate == null) {
                nodeToUpdate = new NodeInfo() { Id = nodeId };
                _nodes.Add(nodeToUpdate);
            }

            nodeToUpdate.AddProperty(propertyId);
        }

        private class NodeInfo {
            internal string Id { get; set; } = "no-id";
            internal string Name { get; set; } = "no-name";
            internal string Type { get; set; } = "no-type";
            public string Properties { get; private set; } = "";

            internal void AddProperty(string propertyId) {
                if (Properties == "") {
                    Properties = propertyId;
                }
                else {
                    Properties += "," + propertyId;
                }
            }
        }

        #endregion
    }
}
