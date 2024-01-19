using BMS.Dto;
using BMS.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BMS.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {

        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;


        public UserController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }


        [AllowAnonymous]
        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if a user with the provided email already exists
            var existingUser = _context.Users.FirstOrDefault(u => u.Email == registerDto.Email);
            if (existingUser != null)
            {
                return BadRequest("A user with this email already exists.");
            }

            // Create a new user entity and populate its properties
            var newUser = new User
            {
                Name = registerDto.Name,
                Email = registerDto.Email,
                Password = registerDto.Password // You should hash and store the password securely
            };

            // Add the new user to the database
            _context.Users.Add(newUser);
            _context.SaveChanges();

            return Ok("Registration successful.");
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = _context.Users.SingleOrDefault(u => u.Email == loginDto.Email);

            //if (user == null || !VerifyPasswordHash(password, user.Password))
            if (user == null || !loginDto.Password.Equals(user.Password))
                return BadRequest(new { message = "Username or password is incorrect" });

            return Ok(new { Token = "Bearer " + GenerateJwtToken(user) });

        }


        [Authorize]
        [HttpGet("profile")]
        public IActionResult GetProfile()
        {
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);

            if (user == null)
            {
                return Unauthorized("User not identified from token.");
            }

            var userProfileDto = new UserProfileDto
            {
                Name = user.Name,
                Email = user.Email
            };

            return Ok(userProfileDto);
        }

        [Authorize]
        [HttpPut("profile/update")]
        public IActionResult UpdateUserProfile([FromBody] UpdateUserProfileDto updateUserProfileDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);

            if (user == null)
            {
                return Unauthorized("User not identified from token.");
            }

            // Update the user's name if provided
            if (!string.IsNullOrWhiteSpace(updateUserProfileDto.Name))
            {
                user.Name = updateUserProfileDto.Name;
            }

            // Update the user's email if provided
            if (!string.IsNullOrWhiteSpace(updateUserProfileDto.Email))
            {
                user.Email = updateUserProfileDto.Email;
            }

            // Check if new password and confirmation are provided and match
            if (!string.IsNullOrWhiteSpace(updateUserProfileDto.NewPassword) &&
                !string.IsNullOrWhiteSpace(updateUserProfileDto.ConfirmPassword) &&
                updateUserProfileDto.NewPassword == updateUserProfileDto.ConfirmPassword)
            {
                // Update the user's password (you may want to hash and store the new password securely)
                user.Password = updateUserProfileDto.NewPassword;
            }
            else if (!string.IsNullOrWhiteSpace(updateUserProfileDto.NewPassword) ||
                     !string.IsNullOrWhiteSpace(updateUserProfileDto.ConfirmPassword))
            {
                // If only one of the password fields is provided, return a bad request
                return BadRequest("Both password and confirmation are required to update the password.");
            }

            // Save changes to the database
            _context.SaveChanges();

            return Ok("Profile updated successfully.");
        }


        private string GenerateJwtToken(User user)
        {
            var jwtKey = _configuration.GetSection("Jwt").GetSection("Key").Value;
            var key = Encoding.ASCII.GetBytes(jwtKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
               new Claim(ClaimTypes.Email, user.Email.ToString()),
               new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
               new Claim(ClaimTypes.Name, user.Name.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
