namespace server_NET.DTOs
{
    public class PaymentIntentRequestDto
    {
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "ils"; // Israeli Shekel
        public List<int> PurchaseIds { get; set; } = new();
        public string? Description { get; set; }
    }

    public class PaymentIntentResponseDto
    {
        public string ClientSecret { get; set; } = string.Empty;
        public string PaymentIntentId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
    }

    public class PaymentConfirmationDto
    {
        public string PaymentIntentId { get; set; } = string.Empty;
        public List<int> PurchaseIds { get; set; } = new();
        public string PaymentStatus { get; set; } = string.Empty;
    }
}
