using server_NET.DTOs;
using server_NET.Models;
using server_NET.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace server_NET.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UserService> _logger;
        
        public UserService(IUserRepository userRepository, ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public User? Authenticate(string email, string password)
        {
            _logger.LogDebug("Authenticating user with email: {Email}", email);
            
            var user = _userRepository.GetByEmail(email);
            if (user == null)
            {
                _logger.LogWarning("Authentication failed - user not found: {Email}", email);
                return null;
            }
            
            if (user.PasswordHash != HashPassword(password))
            {
                _logger.LogWarning("Authentication failed - invalid password for user: {Email}", email);
                return null;
            }
            
            _logger.LogInformation("User successfully authenticated: {Email} with role: {Role}", email, user.Role);
            return user;
        }

        public User? GetByEmail(string email)
        {
            _logger.LogDebug("Getting user by email: {Email}", email);
            return _userRepository.GetByEmail(email);
        }

        public User CreateUser(RegisterRequestDto dto)
        {
            _logger.LogInformation("Creating new user with email: {Email}", dto.Email);
            
            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                Phone = dto.Phone ?? string.Empty,
                PasswordHash = HashPassword(dto.Password),
                Role = "customer"
            };
            
            _userRepository.Add(user);
            _logger.LogInformation("User created successfully with ID: {UserId} and email: {Email}", user.Id, user.Email);
            return user;
        }

        public IEnumerable<User> GetAll()
        {
            _logger.LogDebug("Getting all users");
            return _userRepository.GetAll();
        }

        public IEnumerable<UserDto> GetAllUsers()
        {
            _logger.LogDebug("Getting all users as DTOs");
            return _userRepository.GetAll().Select(u => new UserDto
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                Phone = u.Phone,
                Role = u.Role
            });
        }

        public bool DeleteUser(int id)
        {
            _logger.LogInformation("Attempting to delete user with ID: {UserId}", id);
            try
            {
                var user = _userRepository.GetById(id);
                if (user == null)
                {
                    _logger.LogWarning("User not found for deletion: {UserId}", id);
                    return false;
                }

                // Don't allow deleting managers
                if (user.Role == "manager")
                {
                    _logger.LogWarning("Attempted to delete manager user: {UserId}", id);
                    throw new InvalidOperationException("Cannot delete manager users");
                }

                _userRepository.Delete(id);
                _logger.LogInformation("User successfully deleted: {UserId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user: {UserId}", id);
                throw;
            }
        }

        private string HashPassword(string password)
        {
            _logger.LogDebug("Hashing password");
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
