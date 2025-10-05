using Microsoft.AspNetCore.Mvc;
using server_NET.DTOs;
using server_NET.Services;
using server_NET.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;

namespace server_NET.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IConfiguration _config;
        private readonly ILogger<UserController> _logger;
        
        public UserController(IUserService userService, IConfiguration config, ILogger<UserController> logger)
        {
            _userService = userService;
            _config = config;
            _logger = logger;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequestDto dto)
        {
            _logger.LogInformation("User registration attempt for email: {Email}", dto.Email);
            
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password) || string.IsNullOrWhiteSpace(dto.Name))
            {
                _logger.LogWarning("Registration failed: Missing required fields for email: {Email}", dto.Email);
                return BadRequest("Name, email, and password are required.");
            }
            
            if (_userService.GetByEmail(dto.Email) != null)
            {
                _logger.LogWarning("Registration failed: Email already exists: {Email}", dto.Email);
                return BadRequest("Email already exists.");
            }
            
            var user = _userService.CreateUser(dto);
            _logger.LogInformation("User successfully registered with ID: {UserId} and email: {Email}", user.Id, user.Email);
            return Ok(new { user.Id, user.Name, user.Email });
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequestDto dto)
        {
            _logger.LogInformation("User login attempt for email: {Email}", dto.Email);
            
            var user = _userService.Authenticate(dto.Email, dto.Password);
            if (user == null)
            {
                _logger.LogWarning("Login failed: Invalid credentials for email: {Email}", dto.Email);
                return Unauthorized("Invalid email or password.");
            }
            
            var token = GenerateJwtToken(user);
            _logger.LogInformation("User successfully logged in with ID: {UserId} and email: {Email}", user.Id, user.Email);
            return Ok(new { token });
        }

        private string GenerateJwtToken(User user)
        {
            _logger.LogDebug("Generating JWT token for user ID: {UserId}", user.Id);
            
            var jwtConfig = _config.GetSection("JwtConfig");
            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtConfig["Secret"] ?? string.Empty));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };
            var token = new JwtSecurityToken(
                issuer: jwtConfig["Issuer"],
                audience: jwtConfig["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(int.Parse(jwtConfig["ExpiryMinutes"] ?? "60")),
                signingCredentials: creds
            );
            
            _logger.LogDebug("JWT token generated successfully for user ID: {UserId}", user.Id);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // Get all users (managers only)
        [Authorize(Roles = "manager")]
        [HttpGet]
        public IActionResult GetAllUsers()
        {
            try
            {
                _logger.LogInformation("Retrieving all users");
                var users = _userService.GetAllUsers();
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users");
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }

        // Delete user (managers only)
        [Authorize(Roles = "manager")]
        [HttpDelete("{id}")]
        public IActionResult DeleteUser(int id)
        {
            try
            {
                _logger.LogInformation("Attempting to delete user with ID: {UserId}", id);
                var result = _userService.DeleteUser(id);
                if (!result)
                {
                    _logger.LogWarning("User not found for deletion: {UserId}", id);
                    return NotFound("User not found");
                }
                _logger.LogInformation("User successfully deleted: {UserId}", id);
                return Ok("User deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user with ID: {UserId}", id);
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }
    }
}
