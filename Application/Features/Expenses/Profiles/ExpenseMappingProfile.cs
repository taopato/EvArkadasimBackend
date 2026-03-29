// Application/Features/Expenses/Profiles/ExpenseMappingProfile.cs
using AutoMapper;
using Application.Features.Expenses.Dtos;
using Application.Features.Expenses.Commands.CreateExpense;
using Domain.Entities;

namespace Application.Features.Expenses.Profiles
{
    public class ExpenseMappingProfile : Profile
    {
        public ExpenseMappingProfile()
        {
            // Entity -> DTO’lar
            CreateMap<Expense, ExpenseDto>()
                .ForMember(d => d.Tur, m => m.MapFrom(s => s.Tur))
                .ForMember(d => d.KayitTarihi, m => m.MapFrom(s => s.KayitTarihi))
                .ForMember(d => d.OdeyenKullaniciAdi,
                           m => m.MapFrom(s => s.OdeyenUser != null
                                ? (s.OdeyenUser.FirstName + " " + s.OdeyenUser.LastName).Trim()
                                : string.Empty))
                .ForMember(d => d.KaydedenKullaniciAdi,
                           m => m.MapFrom(s => s.KaydedenUser != null
                                ? (s.KaydedenUser.FirstName + " " + s.KaydedenUser.LastName).Trim()
                                : string.Empty));

            CreateMap<Expense, ExpenseListDto>()
                .ForMember(d => d.Tur, m => m.MapFrom(s => s.Tur))
                .ForMember(d => d.KayitTarihi, m => m.MapFrom(s => s.KayitTarihi))
                // 🔹 yeni metadata alanları isimle eşleştiği için ekstra map gerekmiyor
                // .ForMember(d => d.PlanStartMonth, m => m.MapFrom(s => s.PlanStartMonth)) vs. Gerek yok; isimler aynı.
                ;

            CreateMap<Expense, ExpenseDetailDto>()
                .ForMember(d => d.Tur, m => m.MapFrom(s => s.Tur))
                .ForMember(d => d.KayitTarihi, m => m.MapFrom(s => s.KayitTarihi));

            // Command -> Entity (single akış için)
            CreateMap<CreateExpenseCommand, Expense>()
                .ForMember(d => d.Tur, m => m.MapFrom(s => s.Tur))
                .ForMember(d => d.Tutar, m => m.MapFrom(s => s.Tutar))
                .ForMember(d => d.HouseId, m => m.MapFrom(s => s.HouseId))
                .ForMember(d => d.OdeyenUserId, m => m.MapFrom(s => s.OdeyenUserId))
                .ForMember(d => d.KaydedenUserId, m => m.MapFrom(s => s.KaydedenUserId))
                .ForMember(d => d.OrtakHarcamaTutari, m => m.MapFrom(s => s.OrtakHarcamaTutari))
                .ForMember(d => d.KayitTarihi, m => m.MapFrom(s => s.Date == default ? System.DateTime.UtcNow : s.Date));
        }
    }
}
