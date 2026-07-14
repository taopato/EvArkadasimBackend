using Application.Services.Accounts;
using Application.Services.Repositories;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Memory;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;

namespace EvArkadasim.API.Controllers
{
    [ApiController]
    [Route("api/account-deletion")]
    [AllowAnonymous]
    public sealed class AccountDeletionController : ControllerBase
    {
        private const string GenericRequestMessage =
            "Bu e-posta ile aktif bir Roomora hesabı varsa silme bağlantısı gönderildi.";

        private readonly IUserRepository _userRepository;
        private readonly IAccountDeletionService _accountDeletionService;
        private readonly IMailService _mailService;
        private readonly IMemoryCache _cache;
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AccountDeletionController> _logger;
        private readonly byte[] _signingKey;

        public AccountDeletionController(
            IUserRepository userRepository,
            IAccountDeletionService accountDeletionService,
            IMailService mailService,
            IMemoryCache cache,
            IWebHostEnvironment environment,
            IConfiguration configuration,
            ILogger<AccountDeletionController> logger)
        {
            _userRepository = userRepository;
            _accountDeletionService = accountDeletionService;
            _mailService = mailService;
            _cache = cache;
            _environment = environment;
            _configuration = configuration;
            _logger = logger;

            var secret = configuration["TokenOptions:SecurityKey"];
            if (string.IsNullOrWhiteSpace(secret) || secret.StartsWith("CHANGE_ME_", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Hesap silme bağlantıları için TokenOptions:SecurityKey yapılandırılmalıdır.");
            _signingKey = Encoding.UTF8.GetBytes(secret);
        }

        [HttpPost("request")]
        public async Task<IActionResult> RequestDeletion(
            [FromBody] AccountDeletionRequest request,
            CancellationToken cancellationToken)
        {
            var email = request.Email?.Trim().ToLowerInvariant() ?? string.Empty;
            if (!new EmailAddressAttribute().IsValid(email))
                return Accepted(new { message = GenericRequestMessage });

            var throttleKey = $"account-deletion:{Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(email)))}";
            if (_cache.TryGetValue(throttleKey, out _))
                return Accepted(new { message = GenericRequestMessage });
            _cache.Set(throttleKey, true, TimeSpan.FromMinutes(2));

            var user = await _userRepository.GetByEmailAsync(email);
            if (user is { IsActive: true })
            {
                try
                {
                    var token = CreateToken(user.Id);
                    var publicBaseUrl = ResolvePublicBaseUrl();
                    var deletionUrl = $"{publicBaseUrl}/account-deletion.html?token={Uri.EscapeDataString(token)}";
                    var body = $"""
                        <p>Roomora hesabını silme isteği aldık.</p>
                        <p><a href="{deletionUrl}">Hesabımı kalıcı olarak sil</a></p>
                        <p>Bu bağlantı 15 dakika geçerlidir. Bu isteği sen yapmadıysan e-postayı yok sayabilirsin.</p>
                        """;
                    await _mailService.SendEmailAsync(user.Email, "Roomora hesap silme bağlantısı", body);
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "Account deletion email could not be sent for user {UserId}", user.Id);
                }
            }

            return Accepted(new { message = GenericRequestMessage });
        }

        [HttpPost("confirm")]
        public async Task<IActionResult> ConfirmDeletion(
            [FromBody] AccountDeletionConfirmation request,
            CancellationToken cancellationToken)
        {
            if (!TryValidateToken(request.Token, out var userId))
                return BadRequest(new { message = "Silme bağlantısı geçersiz veya süresi dolmuş." });

            try
            {
                var result = await _accountDeletionService.DeleteAsync(userId, cancellationToken);
                DeleteProfileImage(result.ProfileImageUrl);
                return Ok(new { message = "Roomora hesabın ve kişisel bilgilerin silindi." });
            }
            catch (KeyNotFoundException)
            {
                return BadRequest(new { message = "Hesap bulunamadı veya daha önce silinmiş." });
            }
        }

        private string CreateToken(int userId)
        {
            var expiresAt = DateTimeOffset.UtcNow.AddMinutes(15).ToUnixTimeSeconds();
            var nonce = Convert.ToHexString(RandomNumberGenerator.GetBytes(12));
            var payload = Encoding.UTF8.GetBytes($"{userId}:{expiresAt}:{nonce}");
            using var hmac = new HMACSHA256(_signingKey);
            var signature = hmac.ComputeHash(payload);
            return $"{WebEncoders.Base64UrlEncode(payload)}.{WebEncoders.Base64UrlEncode(signature)}";
        }

        private bool TryValidateToken(string? token, out int userId)
        {
            userId = 0;
            if (string.IsNullOrWhiteSpace(token) || token.Length > 1024) return false;

            try
            {
                var parts = token.Split('.', 2);
                if (parts.Length != 2) return false;
                var payload = WebEncoders.Base64UrlDecode(parts[0]);
                var suppliedSignature = WebEncoders.Base64UrlDecode(parts[1]);
                using var hmac = new HMACSHA256(_signingKey);
                var expectedSignature = hmac.ComputeHash(payload);
                if (suppliedSignature.Length != expectedSignature.Length ||
                    !CryptographicOperations.FixedTimeEquals(suppliedSignature, expectedSignature))
                    return false;

                var values = Encoding.UTF8.GetString(payload).Split(':', 3);
                if (values.Length != 3 || !int.TryParse(values[0], out userId) || userId <= 0)
                    return false;
                if (!long.TryParse(values[1], out var expiresAt)) return false;
                return DateTimeOffset.UtcNow.ToUnixTimeSeconds() <= expiresAt;
            }
            catch
            {
                userId = 0;
                return false;
            }
        }

        private string ResolvePublicBaseUrl()
        {
            var configured = _configuration["AppUrls:PublicApiBaseUrl"]?.TrimEnd('/');
            if (!string.IsNullOrWhiteSpace(configured)) return configured;
            return $"{Request.Scheme}://{Request.Host}";
        }

        private void DeleteProfileImage(string? profileImageUrl)
        {
            if (string.IsNullOrWhiteSpace(profileImageUrl) ||
                !profileImageUrl.StartsWith("/uploads/profiles/", StringComparison.OrdinalIgnoreCase))
                return;

            var webRoot = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
            var profileRoot = Path.GetFullPath(Path.Combine(webRoot, "uploads", "profiles"));
            var relative = profileImageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var fullPath = Path.GetFullPath(Path.Combine(webRoot, relative));
            var rootPrefix = profileRoot.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            if (fullPath.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase) && System.IO.File.Exists(fullPath))
                System.IO.File.Delete(fullPath);
        }
    }

    public sealed record AccountDeletionRequest(string? Email);
    public sealed record AccountDeletionConfirmation(string? Token);
}
