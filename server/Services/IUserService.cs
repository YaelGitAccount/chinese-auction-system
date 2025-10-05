using server_NET.DTOs;
using server_NET.Models;
using System.Collections.Generic;

namespace server_NET.Services
{
    public interface IUserService
    {
        User? Authenticate(string email, string password);
        User? GetByEmail(string email);
        User CreateUser(RegisterRequestDto dto);
        IEnumerable<User> GetAll();
        IEnumerable<UserDto> GetAllUsers();
        bool DeleteUser(int id);
    }
}
