using Application.Services.Repositories;
using MediatR;

namespace Application.Features.Houses.Commands.DeleteHouse
{
    public class DeleteHouseCommandHandler : IRequestHandler<DeleteHouseCommand, Unit>
    {
        private readonly IHouseRepository _houseRepo;
        private readonly IHouseMemberRepository _memberRepo;

        public DeleteHouseCommandHandler(IHouseRepository houseRepo, IHouseMemberRepository memberRepo)
        {
            _houseRepo = houseRepo;
            _memberRepo = memberRepo;
        }

        public async Task<Unit> Handle(DeleteHouseCommand request, CancellationToken ct)
        {
            var house = await _houseRepo.GetByIdAsync(request.HouseId)
                ?? throw new KeyNotFoundException("Ev bulunamadı.");

            if (house.CreatorUserId != request.RequestingUserId)
                throw new UnauthorizedAccessException("Sadece ev kurucusu evi silebilir.");

            // Tüm aktif üyeleri soft-delete ile çıkar
            var members = await _memberRepo.GetByHouseIdAsync(request.HouseId);
            foreach (var member in members.Where(m => m.IsActive))
            {
                member.IsActive = false;
                member.LeftAt = DateTime.UtcNow;
                member.RemovedByUserId = request.RequestingUserId;
                await _memberRepo.UpdateAsync(member);
            }

            // Fiziksel silme denemesi: bagli veriler nedeniyle silinemiyorsa
            // uye soft-delete uygulandigi icin ev artik kullanicilarda gorunmez.
            try
            {
                await _houseRepo.DeleteAsync(request.HouseId);
            }
            catch (InvalidOperationException)
            {
                // no-op
            }

            return Unit.Value;
        }
    }
}
