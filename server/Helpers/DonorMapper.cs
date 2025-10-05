using server_NET.DTOs;
using server_NET.Models;

namespace server_NET.Helpers
{
    public static class DonorMapper
    {
        public static DonorDto ToDto(Donor donor)
        {
            return new DonorDto
            {
                Id = donor.Id,
                Name = donor.Name,
                Email = donor.Email,
                Phone = donor.Phone,
                Gifts = donor.Gifts?.Select(g => new GiftDto
                {
                    Id = g.Id,
                    Name = g.Name,
                    CategoryId = g.CategoryId,
                    CategoryName = g.Category?.Name ?? string.Empty,
                    CategoryColor = g.Category?.Color,
                    Price = g.Price,
                    DonorId = donor.Id,
                    DonorName = donor.Name
                }).ToList() ?? new List<GiftDto>()
            };
        }

        public static Donor ToEntity(DonorDto dto)
        {
            return new Donor
            {
                Id = dto.Id,
                Name = dto.Name,
                Email = dto.Email,
                Phone = dto.Phone,
                Gifts = new List<Gift>() // Gifts mapping can be expanded as needed
            };
        }
    }
}
