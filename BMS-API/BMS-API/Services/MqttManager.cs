using BMS.Model;
using Microsoft.EntityFrameworkCore;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Packets;
using MQTTnet.Protocol;
using System.Text.Json;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;


namespace MqttClient
{
    public class MqttManager
    {

        private readonly IMqttClient _mqttClient;
        private readonly MqttClientOptions _options;

        // Storing devices and hubs temporarily
        private Dictionary<string, Hub> _discoveredHubs = new Dictionary<string, Hub>();

        private readonly IServiceScopeFactory _scopeFactory;

        public MqttManager(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;

            string certFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Certs", "emqxsl-ca.crt");


            var mqttFactory = new MqttFactory();

            X509Certificate2 cacert = new X509Certificate2(File.ReadAllBytes(certFilePath));

            _mqttClient = mqttFactory.CreateMqttClient();

            _options = new MqttClientOptionsBuilder().WithTcpServer("u3a01a10.ala.us-east-1.emqxsl.com", 8883)
                .WithCredentials("username", "password")
                .WithTls(
                    new MqttClientOptionsBuilderTlsParameters()
                    {
                        UseTls = true,
                        SslProtocol = System.Security.Authentication.SslProtocols.Tls12,
                        CertificateValidationHandler = (certContext) =>
                        {
                            X509Chain chain = new X509Chain();
                            chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
                            chain.ChainPolicy.RevocationFlag = X509RevocationFlag.ExcludeRoot;
                            chain.ChainPolicy.VerificationFlags = X509VerificationFlags.NoFlag;
                            chain.ChainPolicy.VerificationTime = DateTime.Now;
                            chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 0, 0);
                            chain.ChainPolicy.CustomTrustStore.Add(cacert);
                            chain.ChainPolicy.TrustMode = X509ChainTrustMode.CustomRootTrust;

                            // convert provided X509Certificate to X509Certificate2
                            var x5092 = new X509Certificate2(certContext.Certificate);

                            return chain.Build(x5092);
                        }
                    })
                .Build();
            _mqttClient.ApplicationMessageReceivedAsync += DeviceReadingMessageHandler;
            _mqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic($"hubs/#").Build());
        }


        private async Task SubscribeToActiveHubsAndDevices()
        {
            //using (var scope = _scopeFactory.CreateScope())
            //{
            //    var _context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            //    // Fetch all active hubs
            //    var activeHubs = await _context.Hubs.Where(h => h.IsActive).ToListAsync();

            //    foreach (var hub in activeHubs)
            //    {
            //        try
            //        {
            //            // Subscribe to the hub's summary topic
            //            await _mqttClient.SubscribeAsync(new MqttTopicFilterBuilder()
            //                .WithTopic($"hubs/{hub.SerialNumber}/summary")
            //                .Build());
            //        }
            //        catch (Exception ex)
            //        {
            //            Console.WriteLine($"Error during registration: {ex.Message}");

            //        }



            //            // Subscribe to the device's topic
            //            await _mqttClient.SubscribeAsync(new MqttTopicFilterBuilder()
            //                .WithTopic($"hubs/#")
            //                .Build());

            //    }
            //}
            // Subscribe to the device's topic
            await _mqttClient.SubscribeAsync(new MqttTopicFilterBuilder()
                .WithTopic($"hubs/#")
                .Build());
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {



            using (var timeout = new CancellationTokenSource(5000))
            {
                await _mqttClient.ConnectAsync(_options, timeout.Token);

                Console.WriteLine("The MQTT client is connected.");
            }
            await SubscribeToActiveHubsAndDevices();

            // Event handlers


            // var mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
            //.WithTopicFilter(
            //    f =>
            //    {
            //        f.WithTopic("mqttnet/samples/topic/2");
            //    })
            //.Build();

            // await mqttClient.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None);

            // Console.WriteLine("MQTT client subscribed to topic.");

            // Console.WriteLine("Press enter to exit.");
            // Console.ReadLine();
        }


        //private Task GeneralMessageHandler(MqttApplicationMessageReceivedEventArgs e)
        //{
        //    return Task.Run(() =>
        //    {
        //        Console.WriteLine("### RECEIVED APPLICATION MESSAGE ###");
        //        Console.WriteLine($"+ Topic = {e.ApplicationMessage.Topic}");
        //        Console.WriteLine($"+ Payload = {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");
        //        Console.WriteLine($"+ QoS = {e.ApplicationMessage.QualityOfServiceLevel}");
        //        Console.WriteLine($"+ Retain = {e.ApplicationMessage.Retain}");
        //        Console.WriteLine();
        //    });
        //}



        private Task DeviceReadingMessageHandler(MqttApplicationMessageReceivedEventArgs e)
        {
            return Task.Run(async () =>
            {
                Console.WriteLine("### RECEIVED APPLICATION MESSAGE ###");
                Console.WriteLine($"+ Topic = {e.ApplicationMessage.Topic}");
                Console.WriteLine($"+ Payload = {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");
                Console.WriteLine($"+ QoS = {e.ApplicationMessage.QualityOfServiceLevel}");
                Console.WriteLine($"+ Retain = {e.ApplicationMessage.Retain}");
                Console.WriteLine();


                // Regular expression to match MQTT topic pattern
                var regex = new Regex("^hubs/.*/devices/.*/readings$");

                if (regex.IsMatch(e.ApplicationMessage.Topic))
                {
                    // Extract the hub serial number and device serial number from the topic
                    var topicParts = e.ApplicationMessage.Topic.Split('/');
                    var hubSerialNumber = topicParts[1];
                    var deviceSerialNumber = topicParts[3];

                    // Create scope in order to save the device reading to the database
                    var scope = _scopeFactory.CreateScope();
                    var _context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    // Use the device serial number to find the device
                    var device = await _context.Devices.FirstOrDefaultAsync(d => d.SerialNumber == deviceSerialNumber);
                    if (device == null)
                    {
                        // TODO: Handle the case where the device is not found in the database
                        return;
                    }

                    // Fetch the device's capabilities
                    var deviceCapabilities = await _context.DeviceCapabilities.Where(c => c.DeviceId == device.Id).ToListAsync();

                    // Parse the payload
                    var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                    var payloadData = JsonSerializer.Deserialize<Dictionary<string, string>>(payload);

                    // Create a new DeviceReading object
                    var deviceReading = new DeviceReading
                    {
                        DeviceId = device.Id,
                        Timestamp = DateTimeOffset.Parse(payloadData["Timestamp"]).UtcDateTime,
                        Value = payloadData["Value"]
                    };

                    // Find the capability that matches the reading type
                    foreach (var capability in deviceCapabilities)
                    {
                        var capabilitySchema = JsonSerializer.Deserialize<DeviceReadingCapabilitySchema>(capability.CapabilityType);
                        if (capabilitySchema.SendReadings && capabilitySchema.ReadingTypes.Any(rt => deviceReading.Value.Contains(rt)))
                        {
                            // You have found the matching capability, so you can set DeviceCapabilityId
                            deviceReading.DeviceCapabilityId = capability.Id;

                            // Save the device reading to the database  
                            _context.DeviceReadings.Add(deviceReading);
                            await _context.SaveChangesAsync();

                            // You can break out of the loop once you've found the matching capability
                            break;
                        }
                    }


                }
                else
                {
                    Console.WriteLine("### RECEIVED APPLICATION MESSAGE ###");
                    Console.WriteLine($"+ Topic = {e.ApplicationMessage.Topic}");
                    Console.WriteLine($"+ Payload = {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");
                    Console.WriteLine($"+ QoS = {e.ApplicationMessage.QualityOfServiceLevel}");
                    Console.WriteLine($"+ Retain = {e.ApplicationMessage.Retain}");
                    Console.WriteLine();

                }
                

            });
        }




        public async Task<bool> InitiateDeviceDiscovery(string hubSerialNumber, int userID, int roomID)
        {
            string summaryTopic = $"hubs/{hubSerialNumber}/summary";
            int totalDevicesToDiscover = 0;
            int devicesDiscoveredCount = 0;
            var discoveryCompletionSource = new TaskCompletionSource<bool>();

            
            _mqttClient.ApplicationMessageReceivedAsync += MessageReceived;

            async Task MessageReceived(MqttApplicationMessageReceivedEventArgs e)
            {

                var scope = _scopeFactory.CreateScope();

                var _context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                if (e.ApplicationMessage.Topic == summaryTopic)
                {
                    var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                    var hubSummary = JsonSerializer.Deserialize<HubSummary>(payload);

                    totalDevicesToDiscover = hubSummary.DeviceCount;

                    // Save hub details to the database
                    var hub = await _context.Hubs.FirstOrDefaultAsync(h => h.SerialNumber == hubSerialNumber);
                    if (hub == null)
                    {
                        hub = new Hub
                        {
                            SerialNumber = hubSerialNumber,
                            Name = hubSummary.HubName,
                            UserId = userID,
                            RoomId = roomID
                        };
                        _context.Hubs.Add(hub);
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        hub.Name = hubSummary.HubName;
                        hub.UserId = userID;
                        hub.RoomId = roomID;
                        hub.IsActive = true;
                        await _context.SaveChangesAsync();
                    }
                }
                else if (e.ApplicationMessage.Topic.StartsWith($"hubs/{hubSerialNumber}/devices/"))
                {


                    // Parse the device data
                    var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);

                    var deviceDetails = new DeviceDetails();
                    try
                    {
                        deviceDetails = JsonSerializer.Deserialize<DeviceDetails>(payload);
                        // Continue with the rest of your code
                    }
                    catch (Exception ex)
                    {
                        // Handle the exception or log the details for debugging
                        Console.WriteLine($"Error during deserialization: {ex.Message}");
                    }

                    var existingDevice = await _context.Devices.FirstOrDefaultAsync(d => d.SerialNumber == deviceDetails.SerialNumber);

                    // The device already exists in the database so we don't need to add it
                    // The search continues without increments device counts etc...
                    //if (existingDevice != null) return;

                    devicesDiscoveredCount++;

                    if (existingDevice != null)
                    {
                        if (existingDevice.IsActive) return; // The device is active so we had found it previously

                        // The device already exists in the database so we just need to update it  
                        existingDevice.IsActive = true;
                        existingDevice.DeviceType = deviceDetails.Type;
                        existingDevice.Name = deviceDetails.Name;
                        existingDevice.RoomId = roomID;
                        existingDevice.UserId = userID;
                        existingDevice.Status = deviceDetails.Status;
                        existingDevice.FailureDescription = deviceDetails.FailureDescription;
                    }
                    else
                    {
                        // Create and add device to the database
                        var device = new Device
                        {
                            DeviceType = deviceDetails.Type,
                            Name = deviceDetails.Name,
                            SerialNumber = deviceDetails.SerialNumber,
                            HubId = (await _context.Hubs.FirstOrDefaultAsync(h => h.SerialNumber == hubSerialNumber)).Id,
                            RoomId = roomID,
                            UserId = userID,
                            Status = deviceDetails.Status,
                            FailureDescription = deviceDetails.FailureDescription
                        };
                        _context.Devices.Add(device);
                        existingDevice = device;
                    }

                    await _context.SaveChangesAsync();


                    // Add or Update device capabilities  
                    foreach (var capabilitySchema in deviceDetails.Capabilities)
                    {
                        string capabilitySchemaJson = JsonSerializer.Serialize(capabilitySchema);

                        DeviceCapability capability = null;


                        capability = await _context.DeviceCapabilities.FirstOrDefaultAsync(c => c.DeviceId == existingDevice.Id && c.CapabilityType == capabilitySchemaJson);


                        if (capability == null)
                        {

                            capability = new DeviceCapability
                            {
                                DeviceId = existingDevice.Id,
                                CapabilityType = capabilitySchemaJson,
                                IsActive = true
                            };
                            _context.DeviceCapabilities.Add(capability);
                        }
                        else
                        {
                            capability.IsActive = true;
                        }
                    }

                    await _context.SaveChangesAsync();

                    if (devicesDiscoveredCount >= totalDevicesToDiscover)
                    {
                        devicesDiscoveredCount=0;
                        discoveryCompletionSource.TrySetResult(true);
                    }

                }

            }

            var message = new MqttApplicationMessageBuilder()
                .WithTopic($"hubs/{hubSerialNumber}/command")
                .WithPayload("Announce")
                .Build();

            await _mqttClient.PublishAsync(message);

            // Set up a timeout to wait for messages  
            using (var timeout = new CancellationTokenSource(600000))
            {
                var completedTask = await Task.WhenAny(discoveryCompletionSource.Task, Task.Delay(-1, timeout.Token));
                _mqttClient.ApplicationMessageReceivedAsync -= MessageReceived;

                if (completedTask == discoveryCompletionSource.Task && discoveryCompletionSource.Task.Result)
                {
                    return true;
                }
                return false;
            }

        }


        public async Task RediscoverDevices(string hubSerialNumber, int userID, int roomID)
        {
            var scope = _scopeFactory.CreateScope();
            var _context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Fetch the hub  
            var hub = await _context.Hubs.FirstOrDefaultAsync(h => h.SerialNumber == hubSerialNumber);

            if (hub != null)
            {
                hub.IsActive = false;

                // Fetch all devices associated with the hub  
                var devices = _context.Devices.Where(d => d.HubId == hub.Id);

                // Mark all the devices as inactive  
                foreach (var device in devices)
                {
                    device.IsActive = false;
                }

                await _context.SaveChangesAsync();
            }

            // Rediscover the devices    
            await InitiateDeviceDiscovery(hubSerialNumber, userID, roomID);
        }

        public async Task StopListeningToHub(string hubSerialNumber)
        {
            await _mqttClient.UnsubscribeAsync($"hubs/{hubSerialNumber}/summary");
            await _mqttClient.UnsubscribeAsync($"hubs/{hubSerialNumber}/devices/+");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task SendDeviceCommand(string hubSerialNumber, string deviceSerial, string command)
        {
            var message = new MqttApplicationMessageBuilder()
                .WithTopic($"hubs/{hubSerialNumber}/devices/{deviceSerial}/command")
                .WithPayload(command)
                .Build();

            await _mqttClient.PublishAsync(message);
        }

        private class DeviceDetails
        {
            public string Type { get; set; }
            public string Name { get; set; }
            public string SerialNumber { get; set; }
            public int RoomId { get; set; }
            public int UserId { get; set; }
            public string Status { get; set; }
            public string FailureDescription { get; set; }
            public List<DeviceCapabilitySchema> Capabilities { get; set; }
        }

        public class DeviceCapabilitySchema
        {
            public string Type { get; set; }
            public List<string> Commands { get; set; }
            public List<string> Tags { get; set; }
            public bool SendReadings { get; set; }
            public List<string> ReadingTypes { get; set; }
            public bool? IsNumeric { get; set; }
            public int? MinValue { get; set; }
            public int? MaxValue { get; set; }
        }

        private class HubSummary
        {
            public string HubName { get; set; }
            public int DeviceCount { get; set; }
        }
        public class DeviceReadingCapabilitySchema
        {
            public bool SendReadings { get; set; }
            public List<string> ReadingTypes { get; set; }
        }


    }
}