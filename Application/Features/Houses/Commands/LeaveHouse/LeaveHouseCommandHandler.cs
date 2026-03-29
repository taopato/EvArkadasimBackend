using MediatR;
using Application.Services.Repositories;

namespace Application.Features.Houses.Commands.LeaveHouse
{
    public class LeaveHouseCommandHandler : IRequestHandler<LeaveHouseCommand, Unit>
    {
        private readonly IHouseMemberRepository _memberRepo;
        private readonly IHouseRepository _houseRepo;

        public LeaveHouseCommandHandler(IHouseMemberRepository memberRepo, IHouseRepository houseRepo)
        {
            _memberRepo = memberRepo;
            _houseRepo = houseRepo;
        }

        public async Task<Unit> Handle(LeaveHouseCommand request, CancellationToken ct)
        {
            var member = await _memberRepo.GetByHouseAndUserAsync(request.HouseId, request.UserId)
                ?? throw new KeyNotFoundException("Bu kullanıcı evde bulunamadı.");

            if (!member.IsActive)
                throw new InvalidOperationException("Bu kullanıcı zaten evde değil.");

            // Yetki kontrolü: sadece kendisi ya da ev sahibi (creator) çıkarabilir
            var house = await _houseRepo.GetByIdAsync(request.HouseId)
                ?? throw new KeyNotFoundException("Ev bulunamadı.");

            bool isSelf = request.RequestingUserId == request.UserId;
            bool isCreator = house.CreatorUserId == request.RequestingUserId;

            if (!isSelf && !isCreator)
                throw new UnauthorizedAccessException("Bu işlemi yapmaya yetkiniz yok.");

            // SOFT DELETE
            member.IsActive = false;
            member.LeftAt = DateTime.UtcNow;
            member.RemovedByUserId = request.RequestingUserId;

            await _memberRepo.UpdateAsync(member);
            await _memberRepo.SaveChangesAsync();

            return Unit.Value;
        }
    }
}
