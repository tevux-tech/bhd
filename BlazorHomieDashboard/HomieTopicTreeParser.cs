using System;
using System.Collections.Generic;
using DevBot9.Protocols.Homie;

namespace TestApp {
    public class HomieTopicTreeParser {
        public static List<Device> Parse(string[] input, string baseTopic) {
            var returnTree = new List<Device>();

            var deviceId = input[0].Split('/')[1];

            var deviceBranch = new Device() { Id = deviceId };

            for (var i = 0; i < input.Length; i++) {
                input[i] = StripPrefix(input[i], baseTopic + "/" + deviceId);
            }

            var deviceAttributes = GetStartsWith(input, "$", true);
            var deviceNodes = new string[0];
            foreach (var attribute in deviceAttributes) {
                var parts = attribute.Split(':');

                deviceBranch.Attributes.Add(parts[0], parts[1]);

                if (parts[0] == "$nodes") {
                    deviceNodes = parts[1].Split(',');
                }
            }

            foreach (var node in deviceNodes) {
                var nodeBranch = new Node() { Id = node };
                var nodeTopics = GetStartsWith(input, node, true);
                for (var i = 0; i < nodeTopics.Length; i++) {
                    nodeTopics[i] = StripPrefix(nodeTopics[i], node);
                }

                var nodeAttributes = GetStartsWith(nodeTopics, "$", true);
                var nodesProperties = new string[0];
                foreach (var attribute in nodeAttributes) {
                    var parts = attribute.Split(':');

                    nodeBranch.Attributes.Add(parts[0], parts[1]);

                    if (parts[0] == "$properties") {
                        nodesProperties = parts[1].Split(',');
                    }
                }

                deviceBranch.Nodes.Add(nodeBranch);

                foreach (var property in nodesProperties) {
                    var propertyMetadata = new ClientPropertyMetadata() { PropertyId = property };

                    var propertyTopics = GetStartsWith(nodeTopics, property, true);
                    for (var i = 0; i < propertyTopics.Length; i++) {
                        propertyTopics[i] = StripPrefix(propertyTopics[i], property);
                    }

                    var propertyAttributes = GetStartsWith(propertyTopics, "$", true);
                    bool? tempIsSettable = null;
                    bool? tempIsRetained = null;
                    foreach (var attribute in propertyAttributes) {
                        var parts = attribute.Split(':');

                        if (parts[0] == "$name") {
                            propertyMetadata.Name = parts[1];
                        }

                        if (parts[0] == "$datatype") {
                            var uppercasedPayload = parts[1].Substring(0, 1).ToUpper() + parts[1].Substring(1);
                            if (Enum.TryParse<DataType>(uppercasedPayload, out var parsedType)) {
                                propertyMetadata.DataType = parsedType;
                            }
                        }

                        if (parts[0] == "$format") {
                            propertyMetadata.Format = parts[1];
                        }

                        if (parts[0] == "$settable") {
                            tempIsSettable = bool.Parse(parts[1]);
                        }

                        if (parts[0] == "$retained") {
                            tempIsRetained = bool.Parse(parts[1]);
                        }

                        if ((tempIsSettable != null) && (tempIsRetained != null)) {
                            if ((tempIsSettable == false) && (tempIsRetained == true)) {
                                propertyMetadata.PropertyType = PropertyType.State;
                            }

                            if ((tempIsSettable == true) && (tempIsRetained == false)) {
                                propertyMetadata.PropertyType = PropertyType.Command;
                            }

                            if ((tempIsSettable == true) && (tempIsRetained == true)) {
                                propertyMetadata.PropertyType = PropertyType.Parameter;
                            }

                            if ((tempIsSettable == false) && (tempIsRetained == false)) {
                                throw new ArgumentException("Not allowed by YAHI...");
                            }
                        }

                        if (parts[0] == "$unit") {
                            propertyMetadata.Unit = parts[1];
                        }
                    }

                    foreach (var propertyTopic in propertyTopics) {
                        if (propertyTopic.StartsWith(':')) {
                            propertyMetadata.InitialValue = propertyTopic.Substring(1);
                        }
                    }

                    propertyMetadata.NodeId = node;

                    nodeBranch.Properties.Add(propertyMetadata);
                }
            }

            returnTree.Add(deviceBranch);

            return returnTree;
        }

        public static string StripPrefix(string input, string prefix) {
            var returnValue = "";

            if (input.StartsWith(prefix)) {
                returnValue = input.Substring(prefix.Length);
            }

            if (returnValue.StartsWith("/")) {
                returnValue = returnValue.Substring("/".Length);
            }

            return returnValue;
        }

        public static string[] GetStartsWith(string[] source, string key, bool removeOriginals = false) {
            var pickedStrings = new List<string>();


            for (var i = 0; i < source.Length; i++) {
                if (source[i].StartsWith(key)) {
                    pickedStrings.Add(source[i]);
                    if (removeOriginals) {
                        source[i] = "";
                    }
                }
            }

            return pickedStrings.ToArray();
        }

        public class Device {
            public string Id;
            public List<Node> Nodes = new();
            public Dictionary<string, string> Attributes = new();
        }

        public class Node {
            public string Id;
            public List<ClientPropertyMetadata> Properties = new();
            public Dictionary<string, string> Attributes = new();
        }
    }
}