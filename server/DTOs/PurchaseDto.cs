namespace server_NET.DTOs
{
    public class PurchaseDto
    {
        public int Id { get; set; }
        public int GiftId { get; set; }
        public int UserId { get; set; }
        public int Quantity { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public string Status { get; set; } = string.Empty;
        // --- נוספו שדות להצגה ומיון ---
        public string GiftName { get; set; } = string.Empty;
        public decimal GiftPrice { get; set; }
        // --- שם רוכש ---
        public string BuyerName { get; set; } = string.Empty;
        // --- האם הגרלה בוצעה למתנה זו ---
        public bool IsGiftLotteryCompleted { get; set; }
    }
}
