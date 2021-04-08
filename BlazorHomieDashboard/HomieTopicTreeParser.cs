using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DevBot9.Protocols.Homie;

namespace TestApp {
    public class HomieTopicTreeParser {
        /// <summary>
        /// Parses Homie tree from provided topic and value dump.
        /// </summary>
        /// <param name="input">Each line should follow this format: {topic}:{value}. For example, homie/lightbulb/$homie:4.0.0</param>       
        public static List<Device> Parse(string[] input, string baseTopic) {
            var returnTree = new List<Device>();

            // First, need to figure out ho many devices are in the input dump. Looking for $homie attributes.
            var distinctDevicesRegex = new Regex(@$"^({baseTopic})\/([a-z0-9][a-z0-9-]+)\/(\$homie):(\S+)$");
            foreach (var inputString in input) {
                var regexMatch = distinctDevicesRegex.Match(inputString);
                if (regexMatch.Success) {
                    returnTree.Add(new Device() { Id = regexMatch.Groups[2].Value });
                }
            }

            // Now, iterating over devices we have just found.
            foreach (var device in returnTree) {
                // Filtering out device attributes. We'll get nodes from them.
                var deviceAttributesRegex = new Regex(@$"^({baseTopic})\/({device.Id})\/(\$[a-z0-9][a-z0-9-]+):(.+)$");
                foreach (var inputString in input) {
                    var regexMatch = deviceAttributesRegex.Match(inputString);
                    if (regexMatch.Success) {
                        device.Attributes.Add(regexMatch.Groups[3].Value, regexMatch.Groups[4].Value);
                    }
                }

                // Figuring out nodes we have on this device.
                var nodes = device.Attributes["$nodes"].Split(",");
                foreach (var node in nodes) {
                    var newNode = new Node() { Id = node };
                    device.Nodes.Add(newNode);

                    // Filtering out attributes for this node. We'll get properties from them.
                    var nodeAttributesRegex = new Regex(@$"^({baseTopic})\/({device.Id})\/({node})\/(\$[a-z0-9][a-z0-9-]+):(.+)$");
                    foreach (var inputString in input) {
                        var regexMatch = nodeAttributesRegex.Match(inputString);
                        if (regexMatch.Success) {
                            newNode.Attributes.Add(regexMatch.Groups[4].Value, regexMatch.Groups[5].Value);
                        }
                    }

                    // Figuring out properties we have for this node.
                    var properties = newNode.Attributes["$properties"].Split(",");
                    foreach (var property in properties) {
                        var newProperty = new ClientPropertyMetadata() { NodeId = node, PropertyId = property };
                        newNode.Properties.Add(newProperty);

                        // Parsing property attributes and value.
                        var propertySubtreeRegex = new Regex(@$"^({baseTopic})\/({device.Id})\/({node})\/({property})(\/\$[a-z0-9][a-z0-9-]+)?(:|\/)(.+)$");
                        bool? tempIsSettable = null;
                        bool? tempIsRetained = null;
                        foreach (var inputString in input) {
                            var regexMatch = propertySubtreeRegex.Match(inputString);
                            if (regexMatch.Success) {
                                var key = regexMatch.Groups[5].Value;
                                var value = regexMatch.Groups[7].Value;

                                if (key == "/$name") { newProperty.Name = value; }
                                if (key == "/$datatype") {
                                    var uppercasedPayload = value.Substring(0, 1).ToUpper() + value.Substring(1);
                                    if (Enum.TryParse<DataType>(uppercasedPayload, out var parsedType)) {
                                        newProperty.DataType = parsedType;
                                    };
                                }
                                if (key == "/$format") { newProperty.Format = value; }
                                if (key == "/$settable") { tempIsSettable = bool.Parse(value); }
                                if (key == "/$retained") { tempIsRetained = bool.Parse(value); }

                                if ((tempIsSettable != null) && (tempIsRetained != null)) {
                                    if ((tempIsSettable == false) && (tempIsRetained == true)) { newProperty.PropertyType = PropertyType.State; }
                                    if ((tempIsSettable == true) && (tempIsRetained == false)) { newProperty.PropertyType = PropertyType.Command; }
                                    if ((tempIsSettable == true) && (tempIsRetained == true)) { newProperty.PropertyType = PropertyType.Parameter; }
                                    if ((tempIsSettable == false) && (tempIsRetained == false)) { throw new ArgumentException("Not allowed by YAHI..."); }
                                }

                                if (key == "/$unit") { newProperty.Unit = value; }

                                if (key == "") { newProperty.InitialValue = value; }
                            }
                        }
                    }

                }
            }

            return returnTree;
        }

        public class Device {
            public string Id;
            public List<Node> Nodes = new();
            public Dictionary<string, string> Attributes = new();
            public override string ToString() {
                return Id;
            }
        }
        public class Node {
            public string Id;
            public List<ClientPropertyMetadata> Properties = new();
            public Dictionary<string, string> Attributes = new();
            public override string ToString() {
                return Id;
            }
        }
    }
}
