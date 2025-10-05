using server_NET.DTOs;
using server_NET.Models;
using server_NET.Repositories;
using server_NET.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace server_NET.Services
{
    public class DonorService : IDonorService
    {
        private readonly IDonorRepository _donorRepository;

        public DonorService(IDonorRepository donorRepository)
        {
            _donorRepository = donorRepository;
        }

        public IEnumerable<DonorDto> GetAllDonors()
        {
            return _donorRepository.GetAll().Select(DonorMapper.ToDto);
        }

        public DonorDto? GetDonorById(int id)
        {
            var donor = _donorRepository.GetById(id);
            return donor == null ? null : DonorMapper.ToDto(donor);
        }

        public DonorDto CreateDonor(DonorDto donorDto)
        {
            var donor = DonorMapper.ToEntity(donorDto);
            _donorRepository.Add(donor);
            return DonorMapper.ToDto(donor);
        }

        public bool UpdateDonor(int id, DonorDto donorDto)
        {
            var donor = _donorRepository.GetById(id);
            if (donor == null) return false;
            donor.Name = donorDto.Name;
            donor.Email = donorDto.Email;
            donor.Phone = donorDto.Phone;
            // עדכון מתנות
            if (donorDto.Gifts != null)
            {
                donor.Gifts.Clear();
                foreach (var giftDto in donorDto.Gifts)
                {
                    donor.Gifts.Add(new Gift
                    {
                        Name = giftDto.Name,
                        CategoryId = giftDto.CategoryId,
                        Price = giftDto.Price,
                        DonorId = donor.Id
                    });
                }
            }
            _donorRepository.Update(donor);
            return true;
        }

        public bool DeleteDonor(int id)
        {
            var donor = _donorRepository.GetById(id);
            if (donor == null) return false;
            _donorRepository.Delete(id);
            return true;
        }
    }
}
