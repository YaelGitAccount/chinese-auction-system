using server_NET.DTOs;

namespace server_NET.Services
{
    public interface IDonorService
    {
        IEnumerable<DonorDto> GetAllDonors();
        DonorDto? GetDonorById(int id);
        DonorDto CreateDonor(DonorDto donorDto);
        bool UpdateDonor(int id, DonorDto donorDto);
        bool DeleteDonor(int id);
    }
}
