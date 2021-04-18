using System;
using System.Collections;
using System.Text.RegularExpressions;
using DevBot9.Protocols.Homie;

namespace TestApp {
    public class HomieTopicTreeParser {
        /// <summary>
        /// Parses Homie tree from provided topic and value dump.
        /// </summary>
        /// <param name="input">Each line should follow this format: {topic}:{value}. For example, homie/lightbulb/$homie:4.0.0</param>       
        public static Device[] Parse(string[] input, string baseTopic) {
            Device[] deviceTree;

            // First, need to figure out ho many devices are in the input dump. Looking for $homie attributes.
            var foundDevices = new ArrayList();
            var distinctDevicesRegex = new Regex($@"^({baseTopic})\/([a-z0-9][a-z0-9-]+)\/(\$homie):(\S+)$");
            foreach (var inputString in input) {
                var regexMatch = distinctDevicesRegex.Match(inputString);
                if (regexMatch.Success) {
                    foundDevices.Add(regexMatch.Groups[2].Value);
                }
            }

            // Now, iterating over devices we have just found.
            deviceTree = new Device[foundDevices.Count];
            for (var d = 0; d < foundDevices.Count; d++) {
                var newDevice = new Device() { Id = (string)foundDevices.ToArray()[d] };
                deviceTree[d] = newDevice;

                // Filtering out device attributes. We'll get nodes from them.
                var deviceAttributesRegex = new Regex($@"^({baseTopic})\/({newDevice.Id})\/(\$[a-z0-9][a-z0-9-]+):(.+)$");
                foreach (var inputString in input) {
                    var regexMatch = deviceAttributesRegex.Match(inputString);
                    if (regexMatch.Success) {
                        newDevice.Attributes.Add(regexMatch.Groups[3].Value, regexMatch.Groups[4].Value);
                    }
                }

                // Figuring out nodes we have on this device.
                var nodes = ((string)newDevice.Attributes["$nodes"]).Split(',');
                newDevice.Nodes = new Node[nodes.Length];
                for (var n = 0; n < nodes.Length; n++) {
                    var newNode = new Node() { Id = nodes[n] };
                    newDevice.Nodes[n] = newNode;

                    // Filtering out attributes for this node. We'll get properties from them.
                    var nodeAttributesRegex = new Regex($@"^({baseTopic})\/({newDevice.Id})\/({newNode.Id})\/(\$[a-z0-9][a-z0-9-]+):(.+)$");
                    foreach (var inputString in input) {
                        var regexMatch = nodeAttributesRegex.Match(inputString);
                        if (regexMatch.Success) {
                            newNode.Attributes.Add(regexMatch.Groups[4].Value, regexMatch.Groups[5].Value);
                        }
                    }

                    // Figuring out properties we have for this node.
                    var properties = ((string)newNode.Attributes["$properties"]).Split(',');
                    newNode.Properties = new ClientPropertyMetadata[properties.Length];
                    for (var p = 0; p < properties.Length; p++) {
                        var newProperty = new ClientPropertyMetadata() { NodeId = newNode.Id, PropertyId = properties[p] };
                        newNode.Properties[p] = newProperty;

                        // Parsing property attributes and value.
                        var propertySubtreeRegex = new Regex($@"^({baseTopic})\/({newDevice.Id})\/({newNode.Id})\/({newProperty.PropertyId})(\/\$[a-z0-9][a-z0-9-]+)?(:|\/)(.+)$");
                        var isSettable = false;
                        var isRetained = false;
                        var isSettableReceived = false;
                        var isRetainedReceived = false;

                        foreach (var inputString in input) {
                            var regexMatch = propertySubtreeRegex.Match(inputString);
                            if (regexMatch.Success) {
                                var key = regexMatch.Groups[5].Value;
                                var value = regexMatch.Groups[7].Value;

                                if (key == "/$name") { newProperty.Name = value; }
                                if (key == "/$datatype") {
                                    if (Helpers.TryParseHomieDataType(value, out var parsedType)) {
                                        newProperty.DataType = parsedType;
                                    };
                                }
                                if (key == "/$format") { newProperty.Format = value; }
                                if (key == "/$settable") {
                                    isSettable = Helpers.ParseBool(value);
                                    isSettableReceived = true;
                                }
                                if (key == "/$retained") {
                                    isRetained = Helpers.ParseBool(value);
                                    isRetainedReceived = true;
                                }

                                if ((isSettableReceived == true) && (isRetainedReceived == true)) {
                                    if ((isSettable == false) && (isRetained == true)) { newProperty.PropertyType = PropertyType.State; }
                                    if ((isSettable == true) && (isRetained == false)) { newProperty.PropertyType = PropertyType.Command; }
                                    if ((isSettable == true) && (isRetained == true)) { newProperty.PropertyType = PropertyType.Parameter; }
                                    if ((isSettable == false) && (isRetained == false)) { throw new ArgumentException("Not allowed by YAHI..."); }
                                }

                                if (key == "/$unit") { newProperty.Unit = value; }

                                if (key == "") { newProperty.InitialValue = value; }
                            }
                        }
                    }

                }
            }

            return deviceTree;
        }

        public class Device {
            public string Id;
            public Node[] Nodes;
            public Hashtable Attributes = new Hashtable();
            public override string ToString() {
                return Id;
            }
        }
        public class Node {
            public string Id;
            public ClientPropertyMetadata[] Properties;
            public Hashtable Attributes = new Hashtable();
            public override string ToString() {
                return Id;
            }
        }
    }
}
