using System.Collections.Generic;
using DevBot9.Protocols.Homie;

namespace BlazorHomieDashboard {
    public class HomieDevice {
        public List<HomieNode> Nodes { get; } = new();

        public string Name { get; set; }
        public string State { get; set; }

        private ClientDevice _clientDevice;

        public void Initialize(HomieTopicTreeParser.Device deviceMetadata, Device.PublishToTopicDelegate publish, Device.SubscribeToTopicDelegate subscribe) {
            Name = (string)deviceMetadata.Attributes["$name"];
            State = (string)deviceMetadata.Attributes["$state"];

            _clientDevice = DeviceFactory.CreateClientDevice(deviceMetadata.Id);

            foreach (var nodeMetaData in deviceMetadata.Nodes) {
                var node = new HomieNode();
                node.Name = (string)nodeMetaData.Attributes["$name"];
                Nodes.Add(node);

                foreach (var propertyMetadata in nodeMetaData.Properties) {
                    switch (propertyMetadata.DataType) {
                        case DataType.Integer: {
                                var newProperty = _clientDevice.CreateClientIntegerProperty(propertyMetadata);
                                node.Properties.Add(newProperty);
                                break;
                            }

                        case DataType.Float: {
                                var newProperty = _clientDevice.CreateClientFloatProperty(propertyMetadata);
                                node.Properties.Add(newProperty);
                                break;
                            }

                        case DataType.Boolean: {
                                var newProperty = _clientDevice.CreateClientBooleanProperty(propertyMetadata);
                                node.Properties.Add(newProperty);
                                break;
                            }

                        case DataType.Enum: {
                                var newProperty = _clientDevice.CreateClientEnumProperty(propertyMetadata);
                                node.Properties.Add(newProperty);
                                break;
                            }

                        case DataType.Color: {
                                var newProperty = _clientDevice.CreateClientColorProperty(propertyMetadata);
                                node.Properties.Add(newProperty);
                                break;
                            }

                        case DataType.DateTime:
                            // Now Datetime cannot be just displayed as string property. Data types are checked internally, and an exception is thrown if when trying create a StringProperty with data type DateTime.
                            break;

                        case DataType.String: {
                                var newProperty = _clientDevice.CreateClientStringProperty(propertyMetadata);
                                node.Properties.Add(newProperty);
                                break;
                            }
                    }
                }
            }

            _clientDevice.Initialize(publish, subscribe);
        }

        public void HandlePublishReceived(string topic, string payload) {
            _clientDevice.HandlePublishReceived(topic, payload);
        }
    }
}
