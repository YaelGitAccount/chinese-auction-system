using server_NET.Models;

namespace server_NET.Repositories
{
    public interface IUserRepository
    {
        User? GetByEmail(string email);
        User? GetById(int id);
        IEnumerable<User> GetAll();
        void Add(User user);
        void Update(User user);
        void Delete(int id);
    }
}
