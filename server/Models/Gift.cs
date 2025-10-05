namespace server_NET.Models
{
    public class Gift
    {
        public int Id { get; set; } // EF Core: Auto numbering
        public string Name { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
        public int DonorId { get; set; }
        public Donor? Donor { get; set; }
        public decimal Price { get; set; }
        public bool IsLotteryCompleted { get; set; } = false; // Whether lottery has been drawn for this gift
        public List<Purchase> Purchases { get; set; } = new List<Purchase>();
    }
}
