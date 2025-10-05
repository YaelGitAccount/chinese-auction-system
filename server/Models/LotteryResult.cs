namespace server_NET.Models
{
    public class LotteryResult
    {
        public int Id { get; set; }
        public int GiftId { get; set; }
        public Gift Gift { get; set; } = null!;
        public int WinnerUserId { get; set; }
        public User? WinnerUser { get; set; } // Nullable to fix EF warning
        public DateTime Date { get; set; }
    }
}
