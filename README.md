[![CI](https://github.com/Girdauskas/BlazorHomieDashboard/actions/workflows/CI.yml/badge.svg?branch=Development)](https://github.com/Girdauskas/BlazorHomieDashboard/actions/workflows/CI.yml)

# BHD

Self-hosted web app for basic MQTT [homie](https://homieiot.github.io/)-based device monitoring and control. Since homie devices use standardized MQTT messages, this dashboard automatically generates itself without any configuration.

![Screen shot](images/screen1.png?raw=true)


## Running using docker
Create volume for user data (to preserve dashboards and other data if you update BHD later):
```
docker volume create bhd-data
```

Start newest version:
```
docker pull ghcr.io/tevux-tech/bhd:latest
docker run --rm -it -p 80:80 -e MQTT_SERVER=192.168.2.2 -v bhd-data:/app/data ghcr.io/tevux-tech/bhd:latest
```
