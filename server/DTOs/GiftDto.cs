namespace server_NET.DTOs
{
    public class GiftDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string? CategoryColor { get; set; }
        public decimal Price { get; set; }
        public int DonorId { get; set; }
        public string DonorName { get; set; } = string.Empty;
        public int BuyersCount { get; set; } // TODO: Consider renaming to TicketsCount for clarity
        public string? WinnerName { get; set; }
        public bool IsLotteryCompleted { get; set; }
    }

    public class CreateGiftDto
    {
        public string Name { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public decimal Price { get; set; }
        public int DonorId { get; set; }
    }

    public class UpdateGiftDto
    {
        public string Name { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public decimal Price { get; set; }
        public int DonorId { get; set; }
    }
}
