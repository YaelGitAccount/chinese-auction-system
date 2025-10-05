using server_NET.Models;

namespace server_NET.Repositories
{
    public interface IGiftRepository
    {
        IEnumerable<Gift> GetAll();
        Gift? GetById(int id);
        void Add(Gift gift);
        void Update(Gift gift);
        void Delete(int id);
    }
}
