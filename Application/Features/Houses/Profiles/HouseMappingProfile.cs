using AutoMapper;
using Domain.Entities;
using Application.Features.Houses.Dtos;

namespace Application.Features.Houses.Profiles
{
    public class HouseMappingProfile : Profile
    {
        public HouseMappingProfile()
        {
            CreateMap<House, CreatedHouseDto>().ReverseMap();
        }
    }
}
