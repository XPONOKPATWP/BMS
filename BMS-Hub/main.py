import asyncio
import mqtt as mqtt
import dht11
import sonoff
async def main():

    client = mqtt.connect_mqtt()
    print(client)

    print("dt11 Serial Number: " + dht11.get_serial_number())
    print("sonoff Serial Number: " + sonoff.get_serial_number())
    print("Hub Serial Number: " + mqtt.get_serial_number())

    while True:
        pass
    

if __name__ == "__main__":
    asyncio.get_event_loop().run_until_complete(main())