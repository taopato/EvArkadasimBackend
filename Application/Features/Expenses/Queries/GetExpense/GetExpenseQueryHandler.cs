using MediatR;
using Application.Features.Expenses.Dtos;
using Application.Services.Repositories;
using Domain.Entities;

namespace Application.Features.Expenses.Queries.GetExpense
{
    public class GetExpenseQueryHandler
        : IRequestHandler<GetExpenseQuery, ExpenseDetailDto>
    {
        private readonly IExpenseRepository _repo;
        private readonly IUserRepository _userRepo;

        public GetExpenseQueryHandler(
            IExpenseRepository repo,
            IUserRepository userRepo)
        {
            _repo = repo;
            _userRepo = userRepo;
        }

        public async Task<ExpenseDetailDto> Handle(
            GetExpenseQuery request,
            CancellationToken ct)
        {
            var e = await _repo.GetByIdAsync(request.ExpenseId)
                    ?? throw new KeyNotFoundException("Expense not found");

            // 🔹 Soft-deleted ise görünmesin
            if (!e.IsActive)
                throw new KeyNotFoundException("Expense not found");

            var dto = ExpenseDtoMapper.ToDetailDto(e);

            // Kullanıcı adları
            var payer = await _userRepo.GetByIdAsync(e.OdeyenUserId);
            var creator = await _userRepo.GetByIdAsync(e.KaydedenUserId);
            dto.OdeyenKullaniciAdi = $"{payer?.FirstName} {payer?.LastName}".Trim();
            dto.KaydedenKullaniciAdi = $"{creator?.FirstName} {creator?.LastName}".Trim();
            dto.KayitTarihi = e.CreatedDate;

            // Şahsi harcamalar
            dto.SahsiHarcamalar = e.PersonalExpenses
                .Select(pe => new PersonalExpenseDto
                {
                    UserId = pe.UserId,
                    KullaniciAdi = $"{pe.User.FirstName} {pe.User.LastName}",
                    Tutar = pe.Tutar
                })
                .ToList();

            dto.OrtakHarcamaTutari = e.OrtakHarcamaTutari;

            // 🔹 Notu description'a yansıt
            if (string.IsNullOrWhiteSpace(dto.Description))
                dto.Description = e.Note ?? string.Empty;

            return dto;
        }
    }
}
