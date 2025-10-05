namespace server_NET.Models
{
    public class Donor
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public List<Gift> Gifts { get; set; } = new List<Gift>();
    }
}
