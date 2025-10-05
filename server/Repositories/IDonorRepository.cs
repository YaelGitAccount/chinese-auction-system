using server_NET.Models;

namespace server_NET.Repositories
{
    public interface IDonorRepository
    {
        IEnumerable<Donor> GetAll();
        Donor? GetById(int id);
        void Add(Donor donor);
        void Update(Donor donor);
        void Delete(int id);
    }
}
