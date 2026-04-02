using MediatR;

namespace Application.Features.Houses.Commands.DeleteHouse
{
    /// <summary>
    /// Bir ev grubunu siler. Sadece evin kurucusu (creator) bu işlemi yapabilir.
    /// Tüm aktif üyeler önce soft-delete ile çıkarılır, ardından ev silinir.
    /// </summary>
    public class DeleteHouseCommand : IRequest<Unit>
    {
        /// <summary>Silinecek evin ID'si</summary>
        public int HouseId { get; set; }

        /// <summary>İşlemi yapan kullanıcı (ev kurucusu olmalı)</summary>
        public int RequestingUserId { get; set; }
    }
}
