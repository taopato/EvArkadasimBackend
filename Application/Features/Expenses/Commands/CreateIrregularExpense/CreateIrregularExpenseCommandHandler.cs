using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Application.Features.Expenses.Dtos;
using Application.Services.Repositories;
using Domain.Entities;
using Domain.Enums;
using MediatR;

namespace Application.Features.Expenses.Commands.CreateIrregularExpense
{
    public class CreateIrregularExpenseCommandHandler
        : IRequestHandler<CreateIrregularExpenseCommand, CreatedExpenseSummaryDto>
    {
        private readonly IExpenseRepository _expenseRepo;
        private readonly IHouseMemberRepository _houseMemberRepo;
        private readonly ILedgerLineRepository _ledgerRepo;

        public CreateIrregularExpenseCommandHandler(
            IExpenseRepository expenseRepo,
            IHouseMemberRepository houseMemberRepo,
            ILedgerLineRepository ledgerRepo)
        {
            _expenseRepo = expenseRepo;
            _houseMemberRepo = houseMemberRepo;
            _ledgerRepo = ledgerRepo;
        }

        public async Task<CreatedExpenseSummaryDto> Handle(CreateIrregularExpenseCommand request, CancellationToken ct)
        {
            var m = request.Model;

            if (string.IsNullOrWhiteSpace(m.Tur))
                throw new InvalidOperationException("Harcama adı boş bırakılamaz.");
            if (m.Tutar <= 0)
                throw new InvalidOperationException("Harcama tutarı sıfırdan büyük olmalıdır.");
            if (m.Note?.Length > 512)
                throw new InvalidOperationException("Harcama notu en fazla 512 karakter olabilir.");

            m.Tur = m.Tur.Trim();
            m.Note = string.IsNullOrWhiteSpace(m.Note) ? null : m.Note.Trim();

            var members = await _houseMemberRepo.GetActiveUserIdsAsync(m.HouseId, ct);
            if (members == null || members.Count == 0)
                throw new InvalidOperationException("Aktif ev üyesi bulunamadı.");
            if (!members.Contains(m.KaydedenUserId))
                throw new InvalidOperationException("Harcamayı kaydeden kişi aktif bir ev üyesi olmalıdır.");
            if (!members.Contains(m.OdeyenUserId))
                throw new InvalidOperationException("Ödeyen kişi aktif bir ev üyesi olmalıdır.");

            var personalMap = (m.PersonalItems ?? new List<PersonalItemDto>())
                                .Where(x => x.Amount > 0)
                                .GroupBy(x => x.UserId)
                                .ToDictionary(g => g.Key, g => g.Sum(x => x.Amount));
            if (personalMap.Keys.Any(userId => !members.Contains(userId)))
                throw new InvalidOperationException("Kişisel harcamalar yalnızca aktif ev üyelerine atanabilir.");
            var personalTotal = personalMap.Values.Sum();
            if (personalTotal > m.Tutar)
                throw new InvalidOperationException("Kişisel toplam, toplam tutardan büyük olamaz.");

            var ortak = m.Tutar - personalTotal;

            var postDate = m.PostDate ?? DateTime.UtcNow;
            var period = $"{postDate:yyyy-MM}";

            var entity = new Expense
            {
                Tur = m.Tur,
                Tutar = m.Tutar,
                OrtakHarcamaTutari = ortak,
                HouseId = m.HouseId,
                OdeyenUserId = m.OdeyenUserId,
                KaydedenUserId = m.KaydedenUserId,
                KayitTarihi = postDate,
                IsActive = true,

                Type = ExpenseType.Irregular,
                Category = m.Category,
                PostDate = postDate,
                DueDate = m.DueDate,
                PeriodMonth = period,
                SplitPolicy = m.SplitPolicy,
                ParticipantsJson = null,
                PersonalBreakdownJson = null,
                VisibilityMode = VisibilityMode.OnBillDate,
                PreShareDays = null,
                RecurrenceBatchKey = null,
                Note = m.Note
            };

            var created = await _expenseRepo.AddAsync(entity);
            await _expenseRepo.SaveChangesAsync(); // Id oluşsun

            // LedgerLines (eşit + kişisel)
            var n = members.Count;
            var baseShare = n > 0 ? Math.Round(ortak / n, 2, MidpointRounding.AwayFromZero) : 0m;
            var totalBase = baseShare * n;
            var diff = Math.Round(ortak - totalBase, 2, MidpointRounding.AwayFromZero);

            var adjustUserId = members.FirstOrDefault(uid => uid != m.OdeyenUserId, members.First());

            var ledgerItems = new List<LedgerLine>();
            foreach (var uid in members)
            {
                if (uid == m.OdeyenUserId) continue;

                var personal = personalMap.TryGetValue(uid, out var pVal) ? pVal : 0m;
                var share = uid == adjustUserId
                    ? Math.Round(baseShare + diff, 2, MidpointRounding.AwayFromZero)
                    : baseShare;

                var amount = Math.Round(share + personal, 2, MidpointRounding.AwayFromZero);
                if (amount <= 0) continue;

                ledgerItems.Add(new LedgerLine
                {
                    HouseId = m.HouseId,
                    ExpenseId = created.Id,
                    FromUserId = uid,
                    ToUserId = m.OdeyenUserId,
                    Amount = amount,
                    PostDate = postDate,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
            }

            foreach (var line in ledgerItems)
                await _ledgerRepo.AddAsync(line, ct);

            // her AddAsync çağrısından sonra değil, en sonda:
            await _ledgerRepo.SaveChangesAsync(ct);

            var resp = new CreatedExpenseSummaryDto
            {
                ExpenseId = created.Id,
                HouseId = created.HouseId,
                OdeyenUserId = created.OdeyenUserId,
                Tutar = created.Tutar,
                OrtakHarcamaTutari = created.OrtakHarcamaTutari,
                PostDate = created.PostDate,
                PeriodMonth = created.PeriodMonth,
                Ledger = ledgerItems.Select(l => new CreatedExpenseSummaryDto.LedgerItem
                {
                    FromUserId = l.FromUserId,
                    ToUserId = l.ToUserId,
                    Amount = l.Amount
                }).ToList()
            };
            return resp;
        }
    }
}
