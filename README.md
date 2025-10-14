# MQTT Unity Bridge



## Local Mosquitto Setup

### Windows

First you need an image of the mosquitto broker as well as docker to deploy it locally.

1. Install Docker [here](https://docs.docker.com/desktop/setup/install/windows-install/)
2. Run 

```
docker pull eclipse-mosquitto
```

3. Create an empty mosquitto conftig file at a save location (e.g. C:/mosquitto/mosquitto.conf)
4. Add following lines to the config file

```
listener 1883 0.0.0.0
allow_anonymous true
```

5. Run 


```
docker run -it -d --name mosquitto -p 1883:1883 -p 9001:9001 -v C:\mosquitto:/mosquitto/config eclipse-mosquitto
```

For testing, use the mosquitto installation. 

1. Install Mosquitto [here](https://mosquitto.org/download/)
2. Add following to path variable: 

```
C:/Program Files/mosquitto
```

3. For subscribing run: 

```
mosquitto_sub -h localhost -t topic/test
```

3. For publishing run: 

```
mosquitto_pub -h localhost -t topic/test -m "Hello"
```