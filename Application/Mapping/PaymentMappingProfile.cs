using Application.Features.Payments.Dtos;
using AutoMapper;
using Domain.Entities;

namespace Application.Mapping
{
    public class PaymentMappingProfile : Profile
    {
        public PaymentMappingProfile()
        {
            CreateMap<Payment, PaymentDto>()
                .ForMember(d => d.PaymentMethod, m => m.MapFrom(s => s.PaymentMethod.ToString()))
                .ForMember(d => d.Status, m => m.MapFrom(s => s.Status.ToString()))
                .ForMember(d => d.OdemeTarihi, m => m.MapFrom(s => s.OdemeTarihi))
                .ForMember(d => d.Aciklama, m => m.MapFrom(s => s.Aciklama))
                .ForMember(d => d.BorcluUserName, m => m.MapFrom(s =>
                    s.BorcluUser != null ? (s.BorcluUser.FirstName + " " + s.BorcluUser.LastName).Trim() : string.Empty
                ))
                .ForMember(d => d.AlacakliUserName, m => m.MapFrom(s =>
                    s.AlacakliUser != null ? (s.AlacakliUser.FirstName + " " + s.AlacakliUser.LastName).Trim() : string.Empty
                ));
        }
    }
}
