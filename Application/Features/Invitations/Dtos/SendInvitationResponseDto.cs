namespace Application.Features.Invitations.Dtos;

public class SendInvitationResponseDto
{
    public string InvitationCode { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
}
