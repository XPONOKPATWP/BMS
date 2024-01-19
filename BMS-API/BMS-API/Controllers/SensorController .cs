using BMS.Model;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BMS.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SensorController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SensorController(AppDbContext context)
        {
            _context = context;
        }

        // Add a new sensor for the authenticated user
        //[HttpPost("add")]
        //public IActionResult AddSensor([FromBody] Sensor sensor)
        //{
        //    var userEmail = User.FindFirst(ClaimTypes.Name)?.Value;
        //    var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);

        //    if (user == null)
        //    {
        //        return Unauthorized("User not identified from token.");
        //    }

        //    sensor.Id = user.Id;
        //    _context.Sensors.Add(sensor);
        //    _context.SaveChanges();

        //    return Ok(sensor);
        //}

        // Retrieve all sensors for the authenticated user
        //[HttpGet("mySensors")]
        //public IActionResult GetMySensors()
        //{
        //    var userEmail = User.FindFirst(ClaimTypes.Name)?.Value;
        //    var user = _context.Users.FirstOrDefault(user => user.Email == userEmail);

        //    if (user == null)
        //    {
        //        return Unauthorized("User not identified from token.");
        //    }

        //    var sensors = _context.Sensors.Where(sensor => sensor.User.Id == user.Id).ToList();

        //    return Ok(sensors);
        //}


        //// Retrieve all the Devices from the eWeLinkApi -- Sonoff
        //[HttpGet("eWeLinkApiDevices")]
        //public async Task<IActionResult> eWeLinkApiGetDecivesAsync()
        //{


        //    return Ok();
        //}

        //// Add data to a specific sensor
        //[HttpPost("{sensorId}/data")]
        //public IActionResult AddSensorData(int sensorId, [FromBody] SensorData data)
        //{
        //    var sensor = _context.Sensors.Find(sensorId);
        //    if (sensor == null)
        //    {
        //        return NotFound("Sensor not found.");
        //    }

        //    var userEmail = User.FindFirst(ClaimTypes.Name)?.Value;
        //    var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);

        //    if (sensor.User.Id != user.Id)
        //    {
        //        return Unauthorized("This sensor does not belong to you.");
        //    }

        //    data.SensorId = sensorId;
        //    _context.SensorData.Add(data);
        //    _context.SaveChanges();

        //    return Ok(data);
        //}

    }
}
