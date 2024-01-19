using BMS.Dto;
using BMS.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MqttClient;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Json;
using static MqttClient.MqttManager;

namespace BMS.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class HubController : ControllerBase
    {
        private readonly MqttManager _mqttManager;
        private readonly AppDbContext _context;

        public HubController(AppDbContext context, MqttManager mqttManager)
        {
            _context = context;
            _mqttManager = mqttManager;
        }

        [Authorize]
        [HttpPost("initiate-discovery")]
        public async Task<IActionResult> InitiateDiscovery([FromBody] DiscoveryRequest request)
        {

            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            if (request == null || string.IsNullOrEmpty(request.HubSerialNumber) || string.IsNullOrEmpty(request.RoomID.ToString()))
            {
                return BadRequest("HubSerialNumber and RoomID is required.");
            }

            var room = await _context.Rooms.FirstOrDefaultAsync(r => r.Id == request.RoomID);
            if (room == null)
            {
                return BadRequest("Room not found.");
            }
            else if (room.UserId != currentUserId)
            {
                return BadRequest("Room does not belong to the user.");
            }

            // Check if hub belongs to the user
            // Get current user id from token claim and compare with hub user id from db
            // If hub does not belong to the user, return bad request
            var hub = await _context.Hubs.FirstOrDefaultAsync(h => h.SerialNumber == request.HubSerialNumber);
            if (hub != null)
            {
                if (hub.UserId != currentUserId && hub.IsActive)
                {
                    return BadRequest("Hub does not belong to the user.");
                }
            }

            try
            {
                var isSuccess = await _mqttManager.InitiateDeviceDiscovery(request.HubSerialNumber, currentUserId, request.RoomID);

                if (isSuccess)
                {
                    return Ok("Device discovery completed successfully.");
                }
                else
                {
                    return BadRequest("Device discovery failed or timed out.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }



        [Authorize]
        [HttpPost("rediscover-hub/{hubSerialNumber}")]
        public async Task<IActionResult> RediscoverDevices(string hubSerialNumber)
        {
            if (string.IsNullOrEmpty(hubSerialNumber))
            {
                return BadRequest("HubSerialNumber is required.");
            }

            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var hub = await _context.Hubs.FirstOrDefaultAsync(h => h.SerialNumber == hubSerialNumber);

            if (hub == null)
            {
                return BadRequest("Cannot Rediscover a non existing Device");
            }
            else
            {
                if (hub.UserId != currentUserId && hub.IsActive)
                {
                    return BadRequest("Hub does not belong to the user.");
                }
            }

            try
            {
                await _mqttManager.RediscoverDevices(hubSerialNumber, currentUserId, hub.RoomId);
                return Ok("Device rediscovery initiated.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [Authorize]
        [HttpDelete("delete-hub/{hubSerialNumber}")]
        public async Task<IActionResult> DeleteHub(string hubSerialNumber)
        {
            if (string.IsNullOrEmpty(hubSerialNumber))
            {
                return BadRequest("HubSerialNumber is required.");
            }

            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);


            try
            {
                var hub = await _context.Hubs.FirstOrDefaultAsync(h => h.SerialNumber == hubSerialNumber);

                if (hub != null)
                {
                    if (hub.UserId != currentUserId && hub.IsActive)
                    {
                        return BadRequest("Hub does not belong to the user.");
                    }
                }
                else
                {
                    return NotFound("Hub not found.");
                }

                hub.IsActive = false;
                var devices = await _context.Devices.Where(d => d.HubId == hub.Id).ToListAsync();

                foreach (var device in devices)
                {
                    device.IsActive = false;
                    var capabilities = await _context.DeviceCapabilities.Where(c => c.DeviceId == device.Id).ToListAsync();
                    foreach (var capability in capabilities)
                    {
                        capability.IsActive = false;
                    }
                }

                await _context.SaveChangesAsync();

                await _mqttManager.StopListeningToHub(hubSerialNumber);

                return Ok("Hub and its devices have been deleted successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }

        }


        [Authorize]
        [HttpGet("device-commands/{deviceId}")]
        public async Task<IActionResult> GetDeviceCommands(int deviceId)
        {
            var device = await _context.Devices.FirstOrDefaultAsync(c => c.Id == deviceId);
            if (device == null)
            {
                return NotFound();
            }

            // Get the currently logged in user's Id
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Check if the current user is the one who can access the device
            if (device.UserId != currentUserId)
            {
                return Forbid();
            }

            var deviceCapabilities = await _context.DeviceCapabilities.Where(c => c.DeviceId == deviceId).ToListAsync();

            if (!deviceCapabilities.Any())
            {
                return NotFound();
            }


            // Configure the options for case-insensitive property names
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };

            var allCommands = new List<string>();

            foreach (var deviceCapability in deviceCapabilities)
            {
                // Parse the JSON into a JsonDocument
                var document = JsonDocument.Parse(deviceCapability.CapabilityType);

                // Access the 'Commands' property of the JsonDocument
                if (document.RootElement.TryGetProperty("Commands", out JsonElement commandsElement) && commandsElement.ValueKind == JsonValueKind.Array)
                {
                    var commands = commandsElement.EnumerateArray().Select(element => element.GetString()).ToList();
                    allCommands.AddRange(commands);
                }
            }

            if (!allCommands.Any())
            {
                return BadRequest("Commands property not found or is not an array");
            }

            return Ok(allCommands);
        }

        [Authorize]
        [HttpPost("device-command/{deviceId}")]
        public async Task<IActionResult> SendDeviceCommand(int deviceId, [FromBody] CommandModel commandModel)
        {
            var device = await _context.Devices.FirstOrDefaultAsync(c => c.Id == deviceId);
            if (device == null)
            {
                return NotFound("Device not found.");
            }

            // Get the currently logged in user's Id
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Check if the current user is the one who can access the device
            if (device.UserId != currentUserId)
            {
                return Forbid();
            }

            var deviceCapabilities = await _context.DeviceCapabilities.Where(c => c.DeviceId == deviceId).ToListAsync();

            // Configure the options for case-insensitive property names
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };

            var allCommands = new List<string>();

            foreach (var deviceCapability in deviceCapabilities)
            {
                // Parse the JSON into a JsonDocument
                var document = JsonDocument.Parse(deviceCapability.CapabilityType);

                // Access the 'Commands' property of the JsonDocument
                if (document.RootElement.TryGetProperty("Commands", out JsonElement commandsElement) && commandsElement.ValueKind == JsonValueKind.Array)
                {
                    var commands = commandsElement.EnumerateArray().Select(element => element.GetString()).ToList();
                    allCommands.AddRange(commands);
                }
            }

            // Check if the command is valid
            if (!allCommands.Contains(commandModel.Command))
            {
                return BadRequest("Invalid command.");
            }

            var hub = await _context.Hubs.FirstOrDefaultAsync(hub => hub.Id == device.HubId);
            if (hub == null)
            {
                return BadRequest("Hub is null.");
            }

            try
            {
                await _mqttManager.SendDeviceCommand(hub.SerialNumber, device.SerialNumber, commandModel.Command);
                return Ok("Command sent.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [Authorize]
        [HttpGet("{hubId}/readings")]
        public async Task<ActionResult<IEnumerable<DeviceReadingsDto>>> GetHubReadings(int hubId)
        {
            var devices = await _context.Devices
                .Include(d => d.DeviceCapabilities)
                .Where(d => d.HubId == hubId)
                .ToListAsync();

            var deviceReadings = new List<DeviceReadingsDto>();

            foreach (var device in devices)
            {
                var deviceReadingDto = new DeviceReadingsDto
                {
                    DeviceId = device.Id,
                    DeviceName = device.Name,
                    Status = device.Status,
                };

                foreach (var capability in device.DeviceCapabilities.Where(dc => dc.IsActive))
                {
                    var capabilityInfo = JsonSerializer.Deserialize<DeviceCapabilitySchema>(capability.CapabilityType);

                    if (capabilityInfo.SendReadings)
                    {
                        foreach (var readingType in capabilityInfo.ReadingTypes)
                        {
                            var latestReading = await _context.DeviceReadings
                                .Where(dr => dr.DeviceId == device.Id && dr.Value.StartsWith(readingType))
                                .OrderByDescending(dr => dr.Timestamp)
                                .FirstOrDefaultAsync();

                            if (latestReading != null)
                            {
                                var valueParts = latestReading.Value.Split(':');
                                if (valueParts.Length > 1)
                                {
                                    deviceReadingDto.LatestReadings[readingType] = valueParts[1];
                                }
                            }

                            var today = DateTime.UtcNow.Date; // Using UTC date for consistency

                            var todayReadings = await _context.DeviceReadings
                                .Where(dr => dr.DeviceId == device.Id &&
                                             dr.Timestamp.Date == today && // Compare just the date parts
                                             dr.Value.StartsWith(readingType))
                                .ToListAsync();

                            if (todayReadings.Any())
                            {
                                var averageValue = todayReadings
                                    .Select(r =>
                                    {
                                        var parts = r.Value.Split(':');
                                        return parts.Length > 1 && double.TryParse(parts[1], out double val) ? val : 0;
                                    })
                                    .Average();

                                deviceReadingDto.AverageReadings[readingType] = averageValue;
                            }
                        }
                    }
                }

                deviceReadings.Add(deviceReadingDto);
            }

            return deviceReadings;
        }



        public class DiscoveryRequest
        {
            public string HubSerialNumber { get; set; }
            public int RoomID { get; set; }
        }

        public class CommandModel
        {
            public string Command { get; set; }
        }

    }
}
