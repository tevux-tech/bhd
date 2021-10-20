using System.Collections.Generic;
using System.Linq;
using Bhd.Server.Services;
using Bhd.Shared.DTOs;
using DevBot9.Protocols.Homie;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Device = Bhd.Shared.DTOs.Device;

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
        public ActionResult<IEnumerable<Device>> Get() {
            var devices = new List<Device>();

            foreach (var homieClientDevice in _homieService.HomieClientDevices) {
                var device = new Device();
                device.Id = homieClientDevice.DeviceId;
                device.Name = homieClientDevice.Name;
                device.Nodes = $"/api/devices/{device.Id}/nodes";

                switch (homieClientDevice.State) {
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
                        // Unsupported state, skipping.
                        break;
                }
            }

            return devices;
        }

        [HttpPost("Rescan")]
        public void Rescan() {
            _homieService.Rescan();
        }

        [HttpGet("{deviceId}")]
        public ActionResult<Device> GetDevice(string deviceId) {
            var device = Get().Value.FirstOrDefault(d => d.Id == deviceId);

            if (device != null) {
                return device;
            }
            else {
                return NotFound();
            }
        }

        [HttpGet("{deviceId}/Nodes")]
        public ActionResult<IEnumerable<Node>> GetNodes(string deviceId) {
            var homieClientDevice = _homieService.HomieClientDevices.FirstOrDefault(d => d.DeviceId == deviceId);

            if (homieClientDevice == null) {
                return NotFound();
            }

            var nodes = new List<Node>();
            foreach (var homieNode in homieClientDevice.Nodes) {
                var node = new Node();
                node.Name = homieNode.Name;
                node.Id = homieNode.NodeId;

                foreach (var clientPropertyBase in homieNode.Properties) {
                    node.Properties.Add($"/api/devices/{deviceId}/nodes/{node.Id}/properties/{clientPropertyBase.PropertyId.Replace($"{node.Id}/", "")}");
                }

                nodes.Add(node);
            }

            return nodes;
        }

        [HttpGet("{deviceId}/Nodes/{nodeId}")]
        public ActionResult<Node> GetNode(string deviceId, string nodeId) {
            var nodes = GetNodes(deviceId);

            var node = nodes.Value?.FirstOrDefault(n => n.Id == nodeId);

            if (node != null) {
                return node;
            }
            else {
                return NotFound();
            }
        }

        [HttpGet("{deviceId}/Nodes/{nodeId}/Properties")]
        public ActionResult<IEnumerable<string>> GetProperties(string deviceId, string nodeId) {
            var node = GetNode(deviceId, nodeId);

            var properties = node.Value?.Properties;

            if (properties != null) {
                return properties;
            }
            else {
                return NotFound();
            }
        }

        [HttpGet("{deviceId}/Nodes/{nodeId}/Properties/{propertyId}")]
        public ActionResult<Property> GetProperty(string deviceId, string nodeId, string propertyId) {
            var propertyBase = GetPropertyBase(deviceId, nodeId, propertyId);

            if (propertyBase == null) {
                return NotFound();
            }

            var property = PropertyFactory.Create(propertyBase);
            return Ok(property);
        }

        [HttpPut("{deviceId}/Nodes/{nodeId}/Properties/{propertyId}/TextValue")]
        public ActionResult SetTextValue(string deviceId, string nodeId, string propertyId, [FromBody] string textValue) {
            var property = GetPropertyBase(deviceId, nodeId, propertyId);

            if (property == null) {
                return NotFound();
            }

            switch (property) {
                case ClientChoiceProperty choiceProperty:
                    choiceProperty.Value = textValue;
                    return Ok();

                case ClientTextProperty textProperty:
                    textProperty.Value = textValue;
                    return Ok();

                case ClientColorProperty colorProperty:
                    colorProperty.Value = HomieColor.FromRgbString(textValue);
                    return Ok();

                default:
                    return Forbid();
            }
        }

        [HttpPut("{deviceId}/Nodes/{nodeId}/Properties/{propertyId}/NumericValue")]
        public ActionResult SetNumericValue(string deviceId, string nodeId, string propertyId, [FromBody] double numericValue) {
            var property = GetPropertyBase(deviceId, nodeId, propertyId);

            if (property == null) {
                return NotFound();
            }

            switch (property) {
                case ClientNumberProperty numberProperty:
                    numberProperty.Value = numericValue;
                    return Ok();

                default:
                    return Forbid();
            }
        }

        private ClientPropertyBase GetPropertyBase(string deviceId, string nodeId, string propertyId) {
            propertyId = $"{nodeId}/{propertyId}";
            var clientDevice = _homieService.HomieClientDevices.FirstOrDefault(d => d.DeviceId == deviceId);
            var node = clientDevice?.Nodes.FirstOrDefault(n => n.NodeId == nodeId);
            var property = node?.Properties.FirstOrDefault(p => p.PropertyId == propertyId);
            return property;
        }
    }
}
