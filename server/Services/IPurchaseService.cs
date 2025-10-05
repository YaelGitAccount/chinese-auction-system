using server_NET.DTOs;
using System.Collections.Generic;

namespace server_NET.Services
{
    public interface IPurchaseService
    {
        IEnumerable<PurchaseDto> GetAll();
        PurchaseDto? GetById(int id);
        void Add(PurchaseDto purchase);
        void Update(PurchaseDto purchase);
        void Delete(int id);
        bool AddGiftToCart(int userId, int giftId);
        bool Checkout(int userId);
        string RemoveFromCart(int userId, int purchaseId);
        string UpdateCartQuantity(int userId, int purchaseId, string action);
    }
}
