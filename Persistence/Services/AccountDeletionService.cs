using Application.Services.Accounts;
using Core.Security.Hashing;
using Microsoft.EntityFrameworkCore;
using Persistence.Contexts;

namespace Persistence.Services
{
    public sealed class AccountDeletionService : IAccountDeletionService
    {
        private readonly AppDbContext _dbContext;

        public AccountDeletionService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<AccountDeletionResult> DeleteAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(x => x.Id == userId && x.IsActive, cancellationToken)
                ?? throw new KeyNotFoundException("Kullanıcı bulunamadı.");

            var profileImageUrl = user.ProfileImageUrl;
            var originalEmail = user.Email;
            var now = DateTime.UtcNow;

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            var memberships = await _dbContext.HouseMembers
                .Where(x => x.UserId == userId && x.IsActive)
                .ToListAsync(cancellationToken);
            foreach (var membership in memberships)
            {
                membership.IsActive = false;
                membership.LeftAt = now;
                membership.RemovedByUserId = userId;
            }

            var invitations = await _dbContext.Invitations
                .Where(x => x.Email == originalEmail)
                .ToListAsync(cancellationToken);
            var verificationCodes = await _dbContext.VerificationCodes
                .Where(x => x.Email == originalEmail)
                .ToListAsync(cancellationToken);
            _dbContext.Invitations.RemoveRange(invitations);
            _dbContext.VerificationCodes.RemoveRange(verificationCodes);

            user.FirstName = "Silinmiş";
            user.LastName = "Kullanıcı";
            user.Email = $"deleted-{user.Id}-{Guid.NewGuid():N}@roomora.invalid";
            user.PasswordHash = HashingHelper.CreatePasswordHash(Guid.NewGuid().ToString("N"));
            user.PhoneNumber = null;
            user.Iban = null;
            user.ProfileImageUrl = null;
            user.IsActive = false;
            user.DeactivatedAt = now;

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return new AccountDeletionResult(profileImageUrl);
        }
    }
}
