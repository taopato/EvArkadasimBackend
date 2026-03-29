using Application.Features.Invitations.Dtos;
using Core.Utilities.Results;
using MediatR;

namespace Application.Features.Invitations.Commands.SendInvitation;

public class SendInvitationCommand : IRequest<Response<SendInvitationResponseDto>>
{
    public int HouseId { get; set; }
    public string Email { get; set; } = null!;
}
