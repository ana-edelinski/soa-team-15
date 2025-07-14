using AutoMapper;
using StakeholdersService.Domain;
using StakeholdersService.Dtos;

namespace StakeholdersService.Mappers
{
    public class AccountProfile : Profile
    {
        public AccountProfile()
        {
            CreateMap<AccountDto, User>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Username))
                .ForMember(dest => dest.Password, opt => opt.MapFrom(src => src.Password))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => Enum.Parse<UserRole>(src.Role)))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                .ReverseMap();
        }
    }
}
