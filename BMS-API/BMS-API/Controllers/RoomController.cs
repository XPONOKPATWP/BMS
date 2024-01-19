using BMS.Dto;
using BMS.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BMS.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class RoomController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RoomController(AppDbContext context)
        {
            _context = context;
        }


        [Authorize]
        [HttpGet("GetRooms")]
        public async Task<ActionResult<IEnumerable<Room>>> GetRooms()
        {
            // Get the user ID from the current user 
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);

            if (user == null)
            {
                return Unauthorized("User not identified from token.");
            }

            List<Room>? rooms = await _context.Rooms
                .Where(r => r.UserId == user.Id)
                .Include(h=> h.Hubs.Where(hub => hub.IsActive == true)) // Include related hubs
                .ThenInclude(h=> h.Devices) // Include related devices
                .ThenInclude(d => d.DeviceCapabilities) // Include DeviceCapabilities
                .ToListAsync();

            var roomDtos = rooms.Select(r => new RoomDto
            {
                Id = r.Id,
                Name = r.Name,
                BuildingId = r.BuildingId,
                Hubs = r.Hubs.Select(h => new HubDto
                {
                    Id = h.Id,
                    Name = h.Name,
                    SerialNumber = h.SerialNumber,
                    Devices = h.Devices.Select(d => new DeviceDto
                    {
                        Id = d.Id,
                        Name = d.Name,
                        Status = d.Status,
                        FailureDescription = d.FailureDescription,
                        DeviceType = d.DeviceType,
                        DeviceCapabilities = d.DeviceCapabilities.Select(dc => new DeviceCapabilityDto
                        {
                            Id = dc.Id,
                            CapabilityType = dc.CapabilityType
                        }).ToList()
                    }).ToList()
                }).ToList()
            }).ToList();    

            return Ok(roomDtos);
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<Room>> GetRoom(int id)
        {
            // Get the user ID from the current user 
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);

            if (user == null)
            {
                return Unauthorized("User not identified from token.");
            }

            var room = await _context.Rooms
               .Where(r => r.UserId == user.Id && r.Id == id)
               .Include(h => h.Hubs.Where(hub => hub.IsActive == true)) // Include related hubs
               .ThenInclude(h => h.Devices) // Include related devices
               .FirstOrDefaultAsync();

            if (room == null)
            {
                return NotFound(); // Room with the given ID was not found or doesn't belong to the user
            }

            var roomDto = new RoomDto
            {
                Id = room.Id,
                Name = room.Name,
                BuildingId = room.BuildingId,
                Hubs = room.Hubs.Select(h => new HubDto
                {
                    Id = h.Id,
                    Name = h.Name,
                    SerialNumber = h.SerialNumber,
                    Devices = h.Devices.Select(d => new DeviceDto
                    {
                        Id = d.Id,
                        Name = d.Name,
                        Status = d.Status,
                        FailureDescription = d.FailureDescription,
                        DeviceType = d.DeviceType
                    }).ToList()
                }).ToList()
            };

            return Ok(roomDto);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRoom(int id, [FromBody] RoomUpdateDto roomUpdateDto)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Get the user ID from the current user 
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);

            if (user == null)
            {
                return Unauthorized("User not identified from token.");
            }

            var room = await _context.Rooms
                .Where(r => r.UserId == user.Id && r.Id == id)
                .FirstOrDefaultAsync();

            if (room == null)
            {
                return NotFound(); // Room with the given ID was not found or doesn't belong to the user
            }

            // Update the room properties based on the provided roomUpdateDto
            if (!string.IsNullOrWhiteSpace(roomUpdateDto.Name))
            {
                room.Name = roomUpdateDto.Name;
            }

            if (roomUpdateDto.BuildingId.HasValue)
            {
                // Check if the specified building exists and belongs to the user
                var building = await _context.Buildings
                    .Where(b => b.UserId == user.Id && b.Id == roomUpdateDto.BuildingId)
                    .FirstOrDefaultAsync();

                if (building == null)
                {
                    return BadRequest("Invalid BuildingId. The specified building does not exist or doesn't belong to the user.");
                }

                room.BuildingId = roomUpdateDto.BuildingId.Value;
            }

            await _context.SaveChangesAsync();

            return NoContent(); // Successfully updated the room
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<RoomDto>> PostRoom([FromBody] RoomCreateDto roomCreateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Get the user ID from the current user 
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);

            if (user == null)
            {
                return Unauthorized("User not identified from token.");
            }

            // Check if the specified building exists and belongs to the user
            var building = await _context.Buildings
                .Where(b => b.UserId == user.Id && b.Id == roomCreateDto.BuildingId)
                .FirstOrDefaultAsync();

            if (building == null)
            {
                return BadRequest("Invalid BuildingId. The specified building does not exist or doesn't belong to the user.");
            }

            // Create a new room based on the roomCreateDto
            var newRoom = new Room
            {
                Name = roomCreateDto.Name,
                BuildingId = roomCreateDto.BuildingId,
                UserId = user.Id
            };

            _context.Rooms.Add(newRoom);
            await _context.SaveChangesAsync();

            // Map the newly created room to a RoomDto
            var roomDto = new RoomDto
            {
                Id = newRoom.Id,
                Name = newRoom.Name,
                BuildingId =newRoom.BuildingId,
            };

            return CreatedAtAction(nameof(GetRoom), new { id = newRoom.Id }, roomDto);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoom(int id)
        {
            // Get the user ID from the current user 
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);

            if (user == null)
            {
                return Unauthorized("User not identified from token.");
            }

            var room = await _context.Rooms
                .Include(r => r.Hubs) // Include related hubs
                .Where(r => r.UserId == user.Id && r.Id == id)
                .FirstOrDefaultAsync();

            if (room == null)
            {
                return NotFound(); // Room with the given ID was not found or doesn't belong to the user
            }

            if (room.Hubs != null && room.Hubs.Any())
            {
                return BadRequest("Cannot delete a room with attached hubs.");
            }

            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();

            return NoContent(); // Successfully deleted the room
        }
    }
}
