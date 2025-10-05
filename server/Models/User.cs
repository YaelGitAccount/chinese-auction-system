namespace server_NET.Models
{
    public class User
    {
        // Add fields: address, last name, phone and cart
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty; // Phone number
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = "customer"; // "manager" or "customer"
        public List<Purchase> Cart { get; set; } = new List<Purchase>(); // Shopping cart
    }
}
