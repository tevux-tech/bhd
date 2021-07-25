using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using Bhd.Server.Services;
using Bhd.Shared;
using DevBot9.Protocols.Homie;
using Device = Bhd.Shared.Device;
using PropertyType = Bhd.Shared.PropertyType;

namespace Bhd.Server.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    public class DevicesController : ControllerBase {
        private readonly ILogger<DevicesController> _logger;
        private readonly HomieService _homieService;

        public DevicesController(ILogger<DevicesController> logger, HomieService homieService) {
            _logger = logger;
            _homieService = homieService;
        }

        [HttpGet]
        public IEnumerable<Device> Get() {
            var devices = new List<Device>();

            foreach (var homieServiceDynamicConsumer in _homieService.DynamicConsumers) {
                var device = new Device();
                device.DeviceId = homieServiceDynamicConsumer.ClientDevice.DeviceId;
                device.Name = homieServiceDynamicConsumer.ClientDevice.Name;

                switch (homieServiceDynamicConsumer.ClientDevice.State) {
                    case HomieState.Ready:
                        device.State = DeviceState.Ready;
                        devices.Add(device);
                        break;

                    case HomieState.Alert:
                        device.State = DeviceState.Alert;
                        devices.Add(device);
                        break;

                    case HomieState.Lost:
                        device.State = DeviceState.Lost;
                        devices.Add(device);
                        break;

                    default:
                        _logger.LogInformation($"{homieServiceDynamicConsumer.ClientDevice.DeviceId} unsupported state {homieServiceDynamicConsumer.ClientDevice.State}");
                        // Unsupported state, skipping.
                        break;
                }
            }


            return devices;
        }

        [HttpGet("{deviceId}")]
        public Device GetDevice(string deviceId) {
            return Get().First(d => d.DeviceId == deviceId);
        }

        [HttpGet("{deviceId}/Nodes")]
        public IEnumerable<Node> GetNodes(string deviceId) {
            var dynamicConsumer = _homieService.DynamicConsumers.First(d => d.ClientDevice.DeviceId == deviceId);
            var nodes = new List<Node>();
            foreach (var clientDeviceNode in dynamicConsumer.ClientDevice.Nodes) {
                var node = new Node();
                node.Name = clientDeviceNode.Name;
                node.NodeId = clientDeviceNode.NodeId;

                node.Properties = new List<Property>();

                foreach (var clientPropertyBase in clientDeviceNode.Properties) {
                    var property = new Property();
                    property.Id = clientPropertyBase.PropertyId.Replace($"{node.NodeId}/", "");
                    property.Path = $"devices/{deviceId}/nodes/{node.NodeId}/properties/{property.Id}";
                    property.Name = clientPropertyBase.Name;
                    property.Bybis = clientPropertyBase.GetType().ToString();

                    switch (clientPropertyBase) {
                        case ClientNumberProperty numberProperty:
                            property.Type = PropertyType.Number;
                            property.Value = numberProperty.Value;
                            node.Properties.Add(property);
                            break;

                        case ClientChoiceProperty choiceProperty:
                            if (choiceProperty.Type == DevBot9.Protocols.Homie.PropertyType.Command) {
                                property.Type = PropertyType.Choice;
                                property.Choices = choiceProperty.Format.Split(",").ToList();
                                node.Properties.Add(property);
                            } else {
                                property.Type = PropertyType.Text;
                                node.Properties.Add(property);
                            }
                            break;

                        case ClientTextProperty textProperty:
                            property.Type = PropertyType.Text;
                            node.Properties.Add(property);
                            break;

                        default:
                            node.Properties.Add(property);
                            break;
                    }
                }

                nodes.Add(node);
            }

            return nodes;
        }

        [HttpGet("{deviceId}/Nodes/{nodeId}")]
        public Node GetNode(string deviceId, string nodeId) {
            var nodes = GetNodes(deviceId);
            return nodes.First(n => n.NodeId == nodeId);
        }

        [HttpGet("{deviceId}/Nodes/{nodeId}/Properties")]
        public IEnumerable<Property> GetProperties(string deviceId, string nodeId) {
            var node = GetNode(deviceId, nodeId);
            return node.Properties;
        }

        [HttpGet("{deviceId}/Nodes/{nodeId}/Properties/{propertyId}")]
        public Property GetProperty(string deviceId, string nodeId, string propertyId) {
            var properties = GetProperties(deviceId, nodeId);
            return properties.First(p => p.Id == propertyId);
        }
    }
}