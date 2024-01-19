import os
import random
import uuid

# Generate a unique serial number
serial_number = str(uuid.uuid4())

def get_serial_number():
    return serial_number

# Check if the current OS is not Linux (like Windows)
if os.name != 'posix':
    # Mock the DHT11 sensor reading function
    def read_dht11():
        # Generate random temperature and humidity for testing
        temperature = round(random.uniform(20, 30), 1)
        humidity = round(random.uniform(40, 60), 1)
        print(f"Temp={temperature}*C  Humidity={humidity}%")
        return temperature, humidity
else:
    import Adafruit_DHT
    import time

    # Define the sensor type and the pin it's connected to
    DHT_SENSOR = Adafruit_DHT.DHT11
    DHT_PIN = 4  # Assuming you connected the DHT11 to GPIO27

    def read_dht11():
        # Try to grab a sensor reading.
        # Note that sometimes you'll receive a None reading, so check for valid data.
        humidity, temperature = Adafruit_DHT.read_retry(DHT_SENSOR, DHT_PIN)

        if humidity is not None and temperature is not None:
            print(f"Temp={temperature:0.1f}*C  Humidity={humidity:0.1f}%")
            return temperature, humidity
        else:
            print("Failed to retrieve data from the sensor.")
            return None, None