using System;
using System.Collections;
using System.Text.RegularExpressions;
using DevBot9.Protocols.Homie;

namespace BlazorHomieDashboard {
    public class HomieTopicTreeParser {
        /// <summary>
        /// Parses Homie tree from provided topic and value dump.
        /// </summary>
        /// <param name="input">Each line should follow this format: {topic}:{value}. For example, homie/lightbulb/$homie:4.0.0</param>       
        public static Device[] Parse(string[] input, string baseTopic, out string[] problemList) {
            var tempProblemList = new ArrayList();

            // First, need to figure out ho many devices are in the input dump. Looking for $homie attributes.
            var foundDeviceIds = new ArrayList();
            var distinctDevicesRegex = new Regex($@"^({baseTopic})\/([a-z0-9][a-z0-9-]+)\/(\$homie):(\S+)$");
            foreach (var inputString in input) {
                var regexMatch = distinctDevicesRegex.Match(inputString);
                if (regexMatch.Success) {
                    foundDeviceIds.Add(regexMatch.Groups[2].Value);
                }
            }

            // Grouping topics by device, so we don't have to reiterate over full list over and over again.
            var sortedTopics = new Hashtable();
            foreach (var inputString in input) {
                for (var d = 0; d < foundDeviceIds.Count; d++) {
                    var deviceId = (string)foundDeviceIds[d];
                    // Adding a new device to hashtable, if it is not there yet.
                    if (sortedTopics.Contains(deviceId) == false) { sortedTopics.Add(deviceId, new ArrayList()); }

                    // Adding a relevant topic for that device.
                    if (inputString.StartsWith($@"{baseTopic}/{deviceId}/")) { ((ArrayList)(sortedTopics[deviceId])).Add(inputString); }
                }
            }

            // Now, iterating over devices we have just found and trying our best to parse as much as possible.
            var goodDevices = new ArrayList();
            for (var d = 0; d < foundDeviceIds.Count; d++) {
                var candidateId = (string)foundDeviceIds[d];
                var candidateTopics = (ArrayList)sortedTopics[candidateId];
                var candidateDevice = new Device(candidateId);

                if (candidateDevice.TryParse(candidateTopics, ref tempProblemList)) { goodDevices.Add(candidateDevice); }
            }

            // Converting local temporary lists to final arrays and returning.
            problemList = new string[tempProblemList.Count];
            for (var i = 0; i < tempProblemList.Count; i++) {
                problemList[i] = (string)tempProblemList[i];
            }

            var deviceTree = new Device[goodDevices.Count];
            for (var i = 0; i < goodDevices.Count; i++) {
                deviceTree[i] = (Device)goodDevices[i];
            }

            return deviceTree;
        }

        public class Device {
            private string _baseTopic;

            public string Id { get; internal set; }
            public Node[] Nodes { get; internal set; }
            public Hashtable Attributes { get; internal set; } = new Hashtable();

            public Device(string id, string baseTopic = "homie") {
                Id = id;
                _baseTopic = baseTopic;
            }

            public bool TryParse(ArrayList topicList, ref ArrayList problemList) {
                var isParsedWell = false;

                var attributesGood = TryParseAttributes(topicList);
                if (attributesGood) {
                    var nodesGood = TryParseNodes(topicList, ref problemList);
                    if (nodesGood) {
                        var propertiesGood = TryParseProperties(topicList, ref problemList);
                        if (propertiesGood) {
                            TrimRottenBranches(ref problemList);
                            isParsedWell = true;
                        } else {
                            problemList.Add($"Device '{Id}' was detected, but it does not have a single valid property. This device subtree will be skipped.");
                        }
                    } else {
                        problemList.Add($"Device '{Id}' was detected, but it does not have a single valid node. This device subtree will be skipped.");
                    }
                } else {
                    problemList.Add($"Device '{Id}' was detected, but it is missing important attributes. This device subtree will be skipped.");
                }

                return isParsedWell;
            }

            private bool TryParseAttributes(ArrayList topicList) {
                var isParseSuccessful = false;

                // Filtering out device attributes. We'll get nodes from them.
                var deviceAttributesRegex = new Regex($@"^({_baseTopic})\/({Id})\/(\$[a-z0-9][a-z0-9-]+):(.+)$");
                var isDeviceNameReceived = false;
                var isNodesReceived = false;
                var isStateReceived = false;

                foreach (string inputString in topicList) {
                    var regexMatch = deviceAttributesRegex.Match(inputString);
                    if (regexMatch.Success) {
                        var key = regexMatch.Groups[3].Value;
                        var value = regexMatch.Groups[4].Value;

                        if (key == "$name") { isDeviceNameReceived = true; }

                        if (key == "$nodes") { isNodesReceived = true; }

                        if (key == "$state") { isStateReceived = true; }

                        Attributes.Add(key, value);
                    }
                }

                var minimumDeviceSetReceived = isDeviceNameReceived & isNodesReceived & isStateReceived;

                if (minimumDeviceSetReceived) {
                    isParseSuccessful = true;
                } else {
                    isParseSuccessful = false;
                }

                return isParseSuccessful;
            }

            private bool TryParseNodes(ArrayList topicList, ref ArrayList problemList) {
                var isParseSuccessful = true;

                var candidateNodeIds = ((string)Attributes["$nodes"]).Split(',');
                var goodNodes = new ArrayList();

                for (var n = 0; n < candidateNodeIds.Length; n++) {
                    var candidateNode = new Node() { Id = candidateNodeIds[n] };

                    // Filtering out attributes for this node. We'll get properties from them.
                    var nodeAttributesRegex = new Regex($@"^({_baseTopic})\/({Id})\/({candidateNode.Id})\/(\$[a-z0-9][a-z0-9-]+):(.+)$");
                    foreach (string inputString in topicList) {
                        var regexMatch = nodeAttributesRegex.Match(inputString);
                        if (regexMatch.Success) {
                            candidateNode.Attributes.Add(regexMatch.Groups[4].Value, regexMatch.Groups[5].Value);
                        }
                    }

                    // Figuring out properties we have for this node.
                    if (candidateNode.Attributes.Contains("$properties")) {
                        goodNodes.Add(candidateNode);
                    } else {
                        // Something is wrong, an essential topic is missing.
                        problemList.Add($"Node '{candidateNode.Id}' of device '{Id}' is defined, but $properties attribute is missing. This node subtree will be skipped entirely.");
                    }
                }

                // Should be at least one valid node. If not - problem.
                if (goodNodes.Count == 0) isParseSuccessful = false;

                // Converting local temporary lists to final arrays and returning.
                Nodes = new Node[goodNodes.Count];
                for (var i = 0; i < goodNodes.Count; i++) {
                    Nodes[i] = (Node)goodNodes[i];
                }

                return isParseSuccessful;
            }

            private bool TryParseProperties(ArrayList topicList, ref ArrayList problemList) {
                var isParseSuccessful = false;

                for (var n = 0; n < Nodes.Length; n++) {
                    var candidatePropertyIds = ((string)Nodes[n].Attributes["$properties"]).Split(',');
                    var goodProperties = new ArrayList();

                    for (var p = 0; p < candidatePropertyIds.Length; p++) {
                        var candidateProperty = new ClientPropertyMetadata() { NodeId = Nodes[n].Id, PropertyId = candidatePropertyIds[p] };

                        // Parsing property attributes and value.
                        var propertySubtreeRegex = new Regex($@"^({_baseTopic})\/({Id})\/({Nodes[n].Id})\/({candidateProperty.PropertyId})(\/\$?[a-z0-9][a-z0-9-]+)?(:|\/)(.+)$");
                        var isSettable = false;
                        var isRetained = false;
                        var isNameReceived = false;
                        var isDataTypeReceived = false;
                        var isSettableReceived = false;
                        var isRetainedReceived = false;

                        foreach (string inputString in topicList) {
                            var regexMatch = propertySubtreeRegex.Match(inputString);
                            if (regexMatch.Success) {
                                var key = regexMatch.Groups[5].Value;
                                var value = regexMatch.Groups[7].Value;

                                if (key == "/$name") {
                                    candidateProperty.Name = value;
                                    isNameReceived = true;
                                }

                                if (key == "/$datatype") {
                                    if (Helpers.TryParseHomieDataType(value, out var parsedType)) {
                                        candidateProperty.DataType = parsedType;
                                        isDataTypeReceived = true;
                                    }

                                    ;
                                }

                                if (key == "/$format") { candidateProperty.Format = value; }

                                if (key == "/$settable") {
                                    isSettable = Helpers.ParseBool(value);
                                    isSettableReceived = true;
                                }

                                if (key == "/$retained") {
                                    isRetained = Helpers.ParseBool(value);
                                    isRetainedReceived = true;
                                }

                                if (key == "/$unit") { candidateProperty.Unit = value; }

                                if (key == "/set") {
                                    /* Discarding this one. This is a historically cached command, and these should not execute during initialization.*/
                                }

                                if (key == "") { candidateProperty.InitialValue = value; }
                            }
                        }

                        var minimumSetReceived = isNameReceived & isDataTypeReceived & isSettableReceived & isRetainedReceived;

                        if (minimumSetReceived) {
                            // Setting property type.
                            if ((isSettable == false) && (isRetained == true)) {
                                candidateProperty.PropertyType = PropertyType.State;
                                goodProperties.Add(candidateProperty);
                            }

                            if ((isSettable == true) && (isRetained == false)) {
                                candidateProperty.PropertyType = PropertyType.Command;
                                goodProperties.Add(candidateProperty);
                            }

                            if ((isSettable == true) && (isRetained == true)) {
                                candidateProperty.PropertyType = PropertyType.Parameter;
                                goodProperties.Add(candidateProperty);
                            }

                            if ((isSettable == false) && (isRetained == false)) {
                                problemList.Add($"Property '{candidateProperty.PropertyId}' of node '{candidateProperty.NodeId}' of device '{Id}' is has all mandatory fields set, but this retainability and settability configuration is not supported by YAHI. This property will be skipped.");
                            }
                        } else {
                            // Some of the mandatory topic were not received. Can't let this property through.
                            problemList.Add($"Property '{candidateProperty.PropertyId}' of node '{candidateProperty.NodeId}' of device '{Id}' is defined, but mandatory attributes are missing. Entire property subtree will be skipped.");
                        }
                    }

                    // Converting local temporary property lists to final arrays.
                    Nodes[n].Properties = new ClientPropertyMetadata[goodProperties.Count];
                    for (var i = 0; i < goodProperties.Count; i++) {
                        Nodes[n].Properties[i] = (ClientPropertyMetadata)goodProperties[i];
                    }
                }

                // Converting local temporary node lists to final arrays and returning.
                foreach (var node in Nodes) {
                    if (node.Properties.Length > 0) {
                        isParseSuccessful = true;
                    }
                }

                return isParseSuccessful;
            }

            private void TrimRottenBranches(ref ArrayList problemList) {
                var goodNodes = new ArrayList();
                var newNodesValue = "";
                var updateNeeded = false;

                foreach (var node in Nodes) {
                    if (node.Properties.Length > 0) {
                        goodNodes.Add(node);

                        var shouldBeThatManyProperties = ((string)node.Attributes["$properties"]).Split(',').Length;
                        if (node.Properties.Length != shouldBeThatManyProperties) {
                            var newAttributeValue = node.Properties[0].PropertyId;
                            for (var i = 1; i < node.Properties.Length; i++) {
                                newAttributeValue += "," + node.Properties[i].PropertyId;
                            }

                            node.Attributes["$properties"] = newAttributeValue;
                            updateNeeded = true;
                        }
                    } else {
                        updateNeeded = true;
                    }
                }

                if (goodNodes.Count > 0) {
                    var shouldBeThatManyNodes = ((string)Attributes["$nodes"]).Split(',').Length;
                    if (goodNodes.Count != shouldBeThatManyNodes) {
                        newNodesValue = ((Node)goodNodes[0]).Id;
                        for (var i = 1; i < goodNodes.Count; i++) {
                            newNodesValue += "," + ((Node)goodNodes[i]).Id;
                        }

                        updateNeeded = true;
                    }
                } else {
                    throw new InvalidOperationException("There should be at least a single good property at this point. Something is internally wrong with parser.");
                }

                if (updateNeeded) {
                    Attributes["$nodes"] = newNodesValue;
                    Nodes = new Node[goodNodes.Count];
                    for (var i = 0; i < goodNodes.Count; i++) {
                        Nodes[i] = (Node)goodNodes[i];
                    }

                    problemList.Add($"Device {Id} has been trimmed. Values of the fields will not be identical to what's on the broker topics!");
                }
            }

            public override string ToString() {
                return Id;
            }
        }

        public class Node {
            public string Id { get; internal set; }
            public ClientPropertyMetadata[] Properties { get; internal set; }
            public Hashtable Attributes { get; internal set; } = new Hashtable();

            public override string ToString() {
                return Id;
            }
        }
    }
}