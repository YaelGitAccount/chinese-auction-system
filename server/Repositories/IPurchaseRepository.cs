using server_NET.Models;

namespace server_NET.Repositories
{
    public interface IPurchaseRepository
    {
        IEnumerable<Purchase> GetAll();
        Purchase? GetById(int id);
        void Add(Purchase purchase);
        void Update(Purchase purchase);
        void Delete(int id);
        IEnumerable<Purchase> GetPurchasesByGiftId(int giftId);
        
        // Performance optimization methods
        Dictionary<int, int> GetTicketCountsByGiftIds(IEnumerable<int> giftIds);
        Dictionary<int, int> GetAllTicketCounts();
    }
}
