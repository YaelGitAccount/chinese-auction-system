namespace server_NET.Models
{
    public class Purchase
    {
        public int Id { get; set; }
        public int GiftId { get; set; }
        public int UserId { get; set; }
        public int Quantity { get; set; } = 1;
        public DateTime? PurchaseDate { get; set; }
        public string Status { get; set; } = "Draft"; // "Draft" for cart, "Confirmed" for order
        public User? User { get; set; }
        public Gift? Gift { get; set; }
    }
}
