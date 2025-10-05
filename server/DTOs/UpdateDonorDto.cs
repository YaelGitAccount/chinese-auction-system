using System.Collections.Generic;

namespace server_NET.DTOs
{
    public class UpdateDonorDto
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public List<GiftDto> Gifts { get; set; } = new List<GiftDto>();
    }
}
