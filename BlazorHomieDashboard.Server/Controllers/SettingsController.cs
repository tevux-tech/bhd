using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace BlazorHomieDashboard.Server.Controllers {
    [ApiController]
    [Route("[controller]")]
    public class SettingsController {
        [HttpGet]
        public IDictionary Get() {

            var settings = new Dictionary<string, string>();

            settings["MQTT_SERVER"] = Environment.GetEnvironmentVariable("MQTT_SERVER") ?? "127.0.0.1";
            settings["MQTT_SERVER_PORT"] = Environment.GetEnvironmentVariable("MQTT_SERVER_PORT") ?? "9001";
            settings["BASE_TOPIC"] = Environment.GetEnvironmentVariable("BASE_TOPIC") ?? "homie";

            return settings;
        }
    }
}