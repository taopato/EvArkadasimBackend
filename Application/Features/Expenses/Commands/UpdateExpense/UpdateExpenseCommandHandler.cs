using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Features.Expenses.Dtos;
using Application.Services.Repositories;
using Domain.Entities;
using Domain.Enums;
using MediatR;

namespace Application.Features.Expenses.Commands.UpdateExpense
{
    public class UpdateExpenseCommandHandler
        : IRequestHandler<UpdateExpenseCommand, Unit>
    {
        private readonly IExpenseRepository _repo;
        private readonly IPersonalExpenseRepository _personalRepo;
        private readonly IShareRepository _shareRepo;
        private readonly IHouseMemberRepository _houseMemberRepo;
        private readonly ILedgerLineRepository _ledgerRepo;

        public UpdateExpenseCommandHandler(
            IExpenseRepository repo,
            IPersonalExpenseRepository personalRepo,
            IShareRepository shareRepo,
            IHouseMemberRepository houseMemberRepo,
            ILedgerLineRepository ledgerRepo)
        {
            _repo = repo;
            _personalRepo = personalRepo;
            _shareRepo = shareRepo;
            _houseMemberRepo = houseMemberRepo;
            _ledgerRepo = ledgerRepo;
        }

        public async Task<Unit> Handle(UpdateExpenseCommand request, CancellationToken ct)
        {
            var expense = await _repo.GetByIdAsync(request.ExpenseId)
                ?? throw new KeyNotFoundException("Expense not found");

            var paidLines = await _ledgerRepo.GetListAsync(
                l => l.ExpenseId == expense.Id && l.IsActive && l.PaidAmount > 0m,
                ct);

            if (paidLines.Any())
            {
                throw new InvalidOperationException(
                    "Bu harcama için ödenmiş borç satırları bulunduğu için finansal alanlar güncellenemez.");
            }

            var personalItems = NormalizePersonalItems(request.Dto.SahsiHarcamalar);
            var personalTotal = personalItems.Sum(x => x.Tutar);
            var sharedAmount = request.Dto.OrtakHarcamaTutari > 0
                ? request.Dto.OrtakHarcamaTutari
                : Math.Max(0m, request.Dto.Tutar - personalTotal);

            expense.Tur = request.Dto.Tur;
            expense.Tutar = request.Dto.Tutar;
            expense.OrtakHarcamaTutari = sharedAmount;
            expense.SplitPolicy = personalItems.Count > 0 ? PaylasimTuru.KisiBazli : PaylasimTuru.Esit;

            var incomingDesc = request.Dto.Aciklama ?? request.Dto.Note ?? request.Dto.Description;
            if (!string.IsNullOrWhiteSpace(incomingDesc))
            {
                expense.Description = incomingDesc;
                expense.Note = incomingDesc;
            }

            await _repo.UpdateAsync(expense);

            foreach (var personal in expense.PersonalExpenses.ToList())
                await _personalRepo.DeleteAsync(personal);

            foreach (var share in expense.Shares.ToList())
                await _shareRepo.DeleteAsync(share);

            foreach (var personal in personalItems)
            {
                await _personalRepo.AddAsync(new PersonalExpense
                {
                    ExpenseId = expense.Id,
                    UserId = personal.UserId,
                    Tutar = personal.Tutar
                });
            }

            var participants = (await _houseMemberRepo.GetActiveUserIdsAsync(expense.HouseId, ct))
                .Distinct()
                .OrderBy(id => id)
                .ToList();

            foreach (var share in BuildEqualShares(sharedAmount, participants))
            {
                await _shareRepo.AddAsync(new Share
                {
                    ExpenseId = expense.Id,
                    UserId = share.UserId,
                    PaylasimTutar = share.Amount,
                    PaylasimTuru = PaylasimTuru.Esit
                });
            }

            var oldLines = await _ledgerRepo.GetListAsync(
                l => l.ExpenseId == expense.Id && l.IsActive,
                ct);

            foreach (var line in oldLines)
            {
                line.IsActive = false;
                line.UpdatedAt = DateTime.UtcNow;
                await _ledgerRepo.UpdateAsync(line, ct);
            }

            var newLines = BuildLedgerLines(expense, participants, sharedAmount, personalItems);
            if (newLines.Count > 0)
            {
                await _ledgerRepo.AddRangeAsync(newLines, ct);
                await _ledgerRepo.SaveChangesAsync(ct);
            }

            await _shareRepo.SaveChangesAsync();
            await _personalRepo.SaveChangesAsync();

            return Unit.Value;
        }

        private static List<PersonalExpenseDto> NormalizePersonalItems(IEnumerable<PersonalExpenseDto>? items)
        {
            return (items ?? Enumerable.Empty<PersonalExpenseDto>())
                .Where(x => x != null && x.UserId > 0 && x.Tutar > 0)
                .GroupBy(x => x.UserId)
                .Select(g => new PersonalExpenseDto
                {
                    UserId = g.Key,
                    Tutar = g.Sum(x => x.Tutar)
                })
                .OrderBy(x => x.UserId)
                .ToList();
        }

        private static List<(int UserId, decimal Amount)> BuildEqualShares(decimal total, List<int> participants)
        {
            var result = new List<(int UserId, decimal Amount)>();
            if (total <= 0 || participants.Count == 0) return result;

            var baseShare = Math.Round(total / participants.Count, 2, MidpointRounding.AwayFromZero);
            var diff = Math.Round(total - (baseShare * participants.Count), 2, MidpointRounding.AwayFromZero);

            for (var i = 0; i < participants.Count; i++)
            {
                result.Add((participants[i], baseShare + (i == 0 ? diff : 0m)));
            }

            return result;
        }

        private static List<LedgerLine> BuildLedgerLines(
            Expense expense,
            List<int> participants,
            decimal sharedAmount,
            List<PersonalExpenseDto> personalItems)
        {
            var result = new List<LedgerLine>();

            foreach (var share in BuildEqualShares(sharedAmount, participants))
            {
                if (share.UserId == expense.OdeyenUserId || share.Amount <= 0) continue;

                result.Add(new LedgerLine
                {
                    HouseId = expense.HouseId,
                    ExpenseId = expense.Id,
                    FromUserId = share.UserId,
                    ToUserId = expense.OdeyenUserId,
                    Amount = share.Amount,
                    PostDate = expense.CreatedDate,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
            }

            foreach (var personal in personalItems)
            {
                if (personal.UserId == expense.OdeyenUserId || personal.Tutar <= 0) continue;

                result.Add(new LedgerLine
                {
                    HouseId = expense.HouseId,
                    ExpenseId = expense.Id,
                    FromUserId = personal.UserId,
                    ToUserId = expense.OdeyenUserId,
                    Amount = personal.Tutar,
                    PostDate = expense.CreatedDate,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
            }

            return result;
        }
    }
}
