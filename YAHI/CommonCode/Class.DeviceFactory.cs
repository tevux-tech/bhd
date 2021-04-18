using System;
using System.Text.RegularExpressions;

namespace DevBot9.Protocols.Homie {
    /// <summary>
    /// This static class is used to create Client and Host Devices using a factory patttern.
    /// </summary>
    public static class DeviceFactory {
        /// <summary>
        /// Base topic that will be added by default when creating Devices.
        /// </summary>
        public static string BaseTopic { get; private set; } = "homie";

        /// <summary>
        /// Initializes the device factory with a base topic.
        /// </summary>
        /// <param name="baseTopic"></param>
        public static void Initialize(string baseTopic = "homie") {
            if (string.IsNullOrEmpty(baseTopic)) { throw new ArgumentException("Base topic cannot be null or an empty string", nameof(baseTopic)); }
            if (_topicLevelRegex.IsMatch(baseTopic) == false) { throw new ArgumentException("Base topic can only be lowercase letters, numbers and hyphens", nameof(baseTopic)); }

            BaseTopic = baseTopic;
        }

        /// <summary>
        /// Creates a Client Device.
        /// </summary>
        public static ClientDevice CreateClientDevice(string deviceId) {
            if (ValidateTopicLevel(deviceId, out var validationMessage) == false) { throw new ArgumentException(validationMessage, nameof(deviceId)); }

            var returnDevice = new ClientDevice(BaseTopic, deviceId);

            return returnDevice;
        }

        /// <summary>
        /// Creates a Host Device.
        /// </summary>
        public static HostDevice CreateHostDevice(string deviceId, string friendlyName) {
            if (ValidateTopicLevel(deviceId, out var validationMessage) == false) { throw new ArgumentException(validationMessage, nameof(deviceId)); }

            var returnDevice = new HostDevice(BaseTopic, deviceId, friendlyName);

            return returnDevice;
        }

        public static bool ValidateTopicLevel(string topicLevelToValidate, out string validationMessage) {
            var validationPassed = true;
            validationMessage = "Ok";

            if (string.IsNullOrEmpty(topicLevelToValidate)) {
                validationPassed = false;
                validationMessage = "Topic level cannot be null or an empty string";
            }
            else if (_topicLevelRegex.IsMatch(topicLevelToValidate) == false) {
                validationPassed = false;
                validationMessage = "Topic level can only be lowercase letters, numbers and hyphens";
            }

            return validationPassed;
        }

        internal static string _topicLevelRegexString = "^[a-z0-9][a-z0-9-]+$";
        internal static Regex _topicLevelRegex = new Regex("^[a-z0-9][a-z0-9-]+$");
    }
}
