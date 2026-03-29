using MediatR;

namespace Application.Features.Houses.Commands.LeaveHouse
{
    /// <summary>
    /// Bir kullanıcının evden ayrılması veya bir üyenin evden çıkarılması.
    /// Soft delete: HouseMember.IsActive = false
    /// </summary>
    public class LeaveHouseCommand : IRequest<Unit>
    {
        /// <summary>Evden ayrılacak / çıkarılacak üyenin ID'si</summary>
        public int UserId { get; set; }
        /// <summary>Ev ID'si</summary>
        public int HouseId { get; set; }
        /// <summary>İşlemi yapan kullanıcı (kendisi veya ev sahibi)</summary>
        public int RequestingUserId { get; set; }
    }
}
