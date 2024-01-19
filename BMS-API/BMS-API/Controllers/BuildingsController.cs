using BMS.Dto;
using BMS.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Security.Claims;

namespace BMS.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class BuildingController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BuildingController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpGet("GetBuildings")]
        public async Task<ActionResult<IEnumerable<BuildingDto>>> GetBuildings()
        {

            // Get the user ID from the current user 
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);

            if (user == null)
            {
                return Unauthorized("User not identified from token.");
            }

            var buildings = await _context.Buildings
                .Where(b => b.UserId == user.Id)
                .Include(b => b.Rooms) // Include related rooms
                .ToListAsync();

            var buildingDtos = buildings.Select(b => new BuildingDto
            {
                Id = b.Id,
                Name = b.Name,
                Rooms = b.Rooms.Select(r => new RoomDto
                {
                    Id = r.Id,
                    Name = r.Name,
                }).ToList()
            }).ToList();

            return Ok(buildingDtos);

        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<BuildingDto>> GetBuildingDetails(int id)
        {
            // Get the user ID from the current user 
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);

            if (user == null)
            {
                return Unauthorized("User not identified from token.");
            }

            var building = await _context.Buildings
                .Where(b => b.UserId == user.Id && b.Id == id)
                .Include(b => b.Rooms) // Include related rooms
                    .ThenInclude(r => r.Hubs.Where(hub => hub.IsActive == true)) // Include related hubs within rooms
                .FirstOrDefaultAsync();

            if (building == null)
            {
                return NotFound(); // Building with the given ID was not found
            }

            var buildingDto = new BuildingDto
            {
                Id = building.Id,
                Name = building.Name,
                Rooms = building.Rooms.Select(r => new RoomDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Hubs = r.Hubs.Select(h => new HubDto
                    {
                        Id = h.Id,
                        SerialNumber = h.SerialNumber,
                        Name = h.Name,

                    }).ToList()
                }).ToList()
            };

            return Ok(buildingDto);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBuildingName(int id, [FromBody] BuildingNameUpdateDto buildingNameUpdateDto)
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

            var building = await _context.Buildings
                .Where(b => b.UserId == user.Id && b.Id == id)
                .FirstOrDefaultAsync();

            if (building == null)
            {
                return NotFound(); // Building with the given ID was not found
            }

            // Update the building name
            building.Name = buildingNameUpdateDto.Name;

            _context.Update(building);
            await _context.SaveChangesAsync();

            return NoContent(); // Successfully updated the building name
        }

        [HttpPost]
        public async Task<ActionResult<BuildingDto>> CreateBuilding([FromBody] BuildingNameUpdateDto buildingCreateDto)
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

            // Create a new building based on the buildingCreateDto
            var newBuilding = new Building
            {
                Name = buildingCreateDto.Name,
                UserId = user.Id
            };

            _context.Buildings.Add(newBuilding);
            await _context.SaveChangesAsync();

            // Map the newly created building to a BuildingDto
            var buildingDto = new BuildingDto
            {
                Id = newBuilding.Id,
                Name = newBuilding.Name,
            };

            return CreatedAtAction(nameof(GetBuildingDetails), new { id = newBuilding.Id }, buildingDto);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBuilding(int id)
        {
            // Get the user ID from the current user 
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);

            if (user == null)
            {
                return Unauthorized("User not identified from token.");
            }

            var building = await _context.Buildings
                .Where(b => b.UserId == user.Id && b.Id == id)
                .Include(b => b.Rooms) // Include related rooms
                .FirstOrDefaultAsync();

            if (building == null)
            {
                return NotFound(); // Building with the given ID was not found
            }

            if (building.Rooms != null && building.Rooms.Any())
            {
                return BadRequest("Cannot delete a building with rooms.");
            }

            _context.Buildings.Remove(building);
            await _context.SaveChangesAsync();

            return NoContent(); // Successfully deleted the building
        }

    }
}
