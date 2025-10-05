namespace server_NET.DTOs
{
    public class LotteryResultDto
    {
        public int Id { get; set; }
        public int GiftId { get; set; }
        public string GiftName { get; set; } = string.Empty;
        public decimal GiftPrice { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int WinnerUserId { get; set; }
        public string WinnerName { get; set; } = string.Empty;
        public string WinnerEmail { get; set; } = string.Empty;
        public DateTime DrawDate { get; set; }
        public int ParticipantsCount { get; set; }
    }
}
