using AutoMapper;
using Domain.Entities;
using MediatR;
using Application.Features.Houses.Dtos;
using Application.Services.Repositories;

namespace Application.Features.Houses.Commands.CreateHouse
{
    public class CreateHouseCommandHandler
        : IRequestHandler<CreateHouseCommand, CreatedHouseDto>
    {
        private readonly IHouseRepository _houseRepository;
        private readonly IMapper _mapper;

        public CreateHouseCommandHandler(IHouseRepository houseRepository, IMapper mapper)
        {
            _houseRepository = houseRepository;
            _mapper = mapper;
        }

        public async Task<CreatedHouseDto> Handle(CreateHouseCommand request, CancellationToken cancellationToken)
        {
            // 1) Yeni house oluştur
            var entity = new House { Name = request.Name };

            // 2) Kaydet
            var added = await _houseRepository.AddAsync(entity);

            // 3) Oluşturan kullanıcıyı üyeye çevir
            var member = new HouseMember
            {
                HouseId = added.Id,
                UserId = request.CreatorUserId
            };
            await _houseRepository.AddMemberAsync(member);

            // 4) DTO dön
            return _mapper.Map<CreatedHouseDto>(added);
        }
    }
}
