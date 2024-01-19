import random
import time
import threading
import json
import dht11
import sonoff
import uuid
import asyncio
import os
from paho.mqtt import client as mqtt_client
from datetime import datetime
from dht11 import read_dht11



summary = {
        "HubName": "Smart Hub",
        "DeviceCount": 2
        }

thermostat_schema = {}
sonoff_schema = {}




broker = 'u3a01a10.ala.us-east-1.emqxsl.com'
port = 8883

# Generate a Client ID with the publish prefix.
client_id = f'publish-{random.randint(0, 1000)}'
username = 'username1'
password = 'password1'


# Generate a unique serial number
serial_number = str(uuid.uuid4())

def get_serial_number():
    if os.name == 'posix':
        return "123456789"
    else:
        return serial_number


def on_message(client, userdata, msg):
    # If a specific command is received, publish a JSON schema to a specific topic
    if msg.payload.decode() == "Announce":
        time.sleep(1)
        client.publish("hubs/"+get_serial_number()+"/summary", json.dumps(summary))
        time.sleep(1)
        client.publish("hubs/"+get_serial_number()+"/devices/"+sonoff.get_serial_number(), json.dumps(thermostat_schema))
        time.sleep(1)
        client.publish("hubs/"+get_serial_number()+"/devices/"+dht11.get_serial_number(), json.dumps(sonoff_schema))
        print(json.dumps(sonoff_schema))
    elif msg.payload.decode() == "on":
        asyncio.run(sonoff.sonoff_command("g.f.k.stagas@gmail.com", "12345678", 0, "on"))
    elif msg.payload.decode() == "off":
        asyncio.run(sonoff.sonoff_command("g.f.k.stagas@gmail.com", "12345678", 0, "off"))


# Publish readings of Temperature and Humidity to the MQTT broker
def publish_readings(client):
    while True:
        topic = "hubs/"+get_serial_number()+"/devices/"+dht11.get_serial_number()+"/readings"
        temperature, humidity = read_dht11()
        timestamp = datetime.utcnow().isoformat() + "Z"

        temp_msg = json.dumps({"Timestamp": timestamp, "Value": f"temperature:{temperature}"})
        client.publish(topic, temp_msg)
        time.sleep(1)
        humidity_msg = json.dumps({"Timestamp": timestamp, "Value": f"humidity:{humidity}"})
        client.publish(topic, humidity_msg)

        time.sleep(30)


def read_schemas():
    global thermostat_schema,sonoff_schema
    thermostat_schema = {
            "Type": "TemperatureHumidityReader",
            "Name": "Living Room Sensor",
            "SerialNumber": dht11.get_serial_number(),
            "RoomId": 101,
            "UserId": 1,
            "Status": "Active",
            "FailureDescription": "",
            "Capabilities": [
                {
                    "Type": "TemperatureRead",
                    "Commands": ["read"],
                    "Tags": ["temperature"],
                    "SendReadings": True,
                    "ReadingTypes": ["temperature"],
                    "IsNumeric": True,
                    "MinValue": 10,
                    "MaxValue": 30
                },
                {
                    "Type": "HumidityRead",
                    "Commands": ["read"],
                    "Tags": [ "humidity"],
                    "SendReadings": True,
                    "ReadingTypes": ["humidity"],
                    "IsNumeric": True,
                    "MinValue": 0,
                    "MaxValue": 100
                }
            ]
    }

    sonoff_schema = {
            "Type": "Switch",
            "Name": "Switch Living Room",
            "SerialNumber": sonoff.get_serial_number(),
            "RoomId": 101,
            "UserId": 1,
            "Status": "Active",
            "FailureDescription": "",
            "Capabilities": [
                {
                    "Type": "OnOff",
                    "Commands": ["on", "off"],
                    "Tags": ["switch"],
                    "SendReadings": False,
                    "ReadingTypes": ["switch"],
                    "IsNumeric": False,
                    "MinValue": 0,
                    "MaxValue": 0
                }
            ]
    }


    


def connect_mqtt():
    conn_event = threading.Event()

    def on_connect(client, userdata, flags, rc):
        if rc == 0:
            print("Connected to MQTT Broker!")
            conn_event.set()
        else:
            print("Failed to connect, return code %d\n", rc)
            conn_event.set()
        

    client = mqtt_client.Client(client_id)
    client.username_pw_set(username, password)
    client.tls_set(
        ca_certs='Certs/emqxsl-ca.crt'
    )
    client.on_connect = on_connect
    client.connect(broker, port)
    client.loop_start()  # Start the loop

    #Subscribe to this hubs topic 
    client.subscribe("hubs/"+get_serial_number()+"/#")
    
    # Read the schemas from the json files
    read_schemas()

    # Set the on_message callback function
    client.on_message = on_message

    # Start a thread to publish readings
    readings_thread = threading.Thread(target=publish_readings, args=(client,))
    readings_thread.daemon = True  # Daemonize the thread so it exits when the main program ends
    readings_thread.start()

    conn_event.wait()  # Wait for the connection to be established
    return client
