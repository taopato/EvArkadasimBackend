namespace Application.Services.Accounts
{
    public sealed record AccountDeletionResult(string? ProfileImageUrl);

    public interface IAccountDeletionService
    {
        Task<AccountDeletionResult> DeleteAsync(int userId, CancellationToken cancellationToken = default);
    }
}
