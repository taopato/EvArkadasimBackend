using Core.Security.Hashing;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Persistence.Contexts;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace EvArkadasim.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CryptoDemoController : ControllerBase
{
    private const string DemoEmail = "jwt.demo@evarkadasim.local";
    private const string DemoPassword = "KriptoDemo2026!";
    private const string DemoSecret = "KriptolojiDersi_HS256_Demo_Secret_Key_EnAz_32_Byte_2026";

    private readonly AppDbContext _db;

    public CryptoDemoController(AppDbContext db)
    {
        _db = db;
    }

    [HttpPost("seed-user")]
    public async Task<IActionResult> SeedUser()
    {
        var user = await _db.Users.FirstOrDefaultAsync(x => x.Email == DemoEmail);

        if (user == null)
        {
            user = new User
            {
                FirstName = "JWT",
                LastName = "Demo",
                Email = DemoEmail,
                PasswordHash = HashingHelper.CreatePasswordHash(DemoPassword),
                CreatedAt = DateTime.UtcNow,
                RegistrationDate = DateTime.UtcNow,
                IsActive = true
            };

            _db.Users.Add(user);
        }
        else
        {
            user.PasswordHash = HashingHelper.CreatePasswordHash(DemoPassword);
            user.IsActive = true;
            user.DeactivatedAt = null;
        }

        await _db.SaveChangesAsync();

        var parsedHash = SplitStoredPassword(user.PasswordHash);
        return Ok(new
        {
            message = "Demo kullanicisi hazir.",
            login = new { email = DemoEmail, password = DemoPassword },
            database = new
            {
                table = "Users",
                columns = new[] { "Id", "Email", "PasswordHash", "CreatedAt", "IsActive" },
                passwordHashFormat = "salt:hash",
                user.Id,
                user.Email,
                passwordHash = user.PasswordHash,
                salt = parsedHash.Salt,
                hash = parsedHash.Hash
            }
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] DemoLoginRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(x => x.Email == request.Email);
        if (user == null)
        {
            return Unauthorized(new { message = "Kullanici bulunamadi. Once demo kullanicisini olusturun." });
        }

        var passwordParts = SplitStoredPassword(user.PasswordHash);
        var computedHash = ComputePasswordHashWithStoredSalt(request.Password, passwordParts.Salt);
        var passwordMatches = HashingHelper.VerifyPasswordHash(request.Password, user.PasswordHash);

        if (!passwordMatches)
        {
            return Unauthorized(new
            {
                message = "Parola yanlis.",
                databaseStep = "Email ile kullanici bulundu, ayni salt ile hash tekrar hesaplandi fakat hashler eslesmedi.",
                storedHash = passwordParts.Hash,
                computedHash
            });
        }

        var token = CreateHs256Token(user);
        var tokenParts = DecodeJwtParts(token);

        Response.Headers.CacheControl = "no-store";

        return Ok(new
        {
            message = "Login basarili. Parola dogrulandi ve HS256 imzali demo JWT uretildi.",
            loginFlow = new[]
            {
                "1) Kullanici email ve parola girdi.",
                "2) Sunucu Users tablosundan kullaniciyi buldu.",
                "3) PasswordHash alanindaki salt:hash formati ayrildi.",
                "4) Girilen parola ayni salt ile HMACSHA512 hash yapildi.",
                "5) Hesaplanan hash veritabanindaki hash ile eslesti.",
                "6) Sunucu HS256 imzali JWT uretip client'a dondu."
            },
            database = new
            {
                table = "Users",
                user.Id,
                user.Email,
                passwordHash = user.PasswordHash,
                salt = passwordParts.Salt,
                storedHash = passwordParts.Hash,
                computedHash,
                passwordMatches
            },
            jwt = new
            {
                token,
                authorizationHeader = $"Bearer {token}",
                secretForDemo = DemoSecret,
                algorithm = "HS256",
                parts = tokenParts
            }
        });
    }

    [HttpGet("profile")]
    public IActionResult Profile()
    {
        var token = ReadBearerToken();
        if (string.IsNullOrWhiteSpace(token))
        {
            return Unauthorized(new { message = "Authorization: Bearer <token> header'i bulunamadi." });
        }

        var validation = ValidateHs256Token(token);
        if (!validation.IsValid)
        {
            return Unauthorized(new
            {
                message = "Token reddedildi.",
                reason = validation.Error,
                networkDemo = "Bu istek Network sekmesinde Authorization header'i ile gorunur."
            });
        }

        return Ok(new
        {
            message = "Token gecerli. Sunucu bu istege izin verdi.",
            networkDemo = "Request Headers -> Authorization: Bearer <JWT>",
            user = new
            {
                id = validation.Principal!.FindFirstValue(ClaimTypes.NameIdentifier),
                email = validation.Principal!.FindFirstValue(ClaimTypes.Email),
                role = validation.Principal!.FindFirstValue(ClaimTypes.Role)
            }
        });
    }

    [HttpPost("verify-token")]
    public IActionResult VerifyToken([FromBody] VerifyTokenRequest request)
    {
        var parts = DecodeJwtParts(request.Token);
        var validation = ValidateHs256Token(request.Token);

        return Ok(new
        {
            validation.IsValid,
            validation.Error,
            explanation = validation.IsValid
                ? "Header ve payload icin hesaplanan HS256 imzasi token'daki signature ile uyustu."
                : "Token okunabilir olsa bile imza dogrulanamadigi icin guvenilir kabul edilmez.",
            parts
        });
    }

    [HttpPost("tamper-role")]
    public IActionResult TamperRole([FromBody] VerifyTokenRequest request)
    {
        var pieces = request.Token.Split('.');
        if (pieces.Length != 3)
        {
            return BadRequest(new { message = "JWT 3 parcadan olusmali." });
        }

        var payloadJson = Encoding.UTF8.GetString(Base64UrlEncoder.DecodeBytes(pieces[1]));
        var payload = JsonSerializer.Deserialize<Dictionary<string, object>>(payloadJson) ?? new Dictionary<string, object>();
        payload["role"] = "admin";

        var tamperedPayloadJson = JsonSerializer.Serialize(payload);
        var tamperedPayload = Base64UrlEncoder.Encode(Encoding.UTF8.GetBytes(tamperedPayloadJson));
        var tamperedToken = $"{pieces[0]}.{tamperedPayload}.{pieces[2]}";
        var validation = ValidateHs256Token(tamperedToken);

        return Ok(new
        {
            message = "Payload role=admin yapildi ama signature eski birakildi.",
            tamperedToken,
            validation.IsValid,
            validation.Error,
            parts = DecodeJwtParts(tamperedToken)
        });
    }

    private static string CreateHs256Token(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(DemoSecret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: "kriptoloji.demo",
            audience: "kriptoloji.demo",
            claims: new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}".Trim()),
                new Claim(ClaimTypes.Role, "student")
            },
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static (bool IsValid, ClaimsPrincipal? Principal, string? Error) ValidateHs256Token(string token)
    {
        try
        {
            var principal = new JwtSecurityTokenHandler().ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = "kriptoloji.demo",
                ValidAudience = "kriptoloji.demo",
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(DemoSecret)),
                ClockSkew = TimeSpan.FromSeconds(10)
            }, out _);

            return (true, principal, null);
        }
        catch (Exception ex)
        {
            return (false, null, ex.Message);
        }
    }

    private string? ReadBearerToken()
    {
        var authorization = Request.Headers.Authorization.ToString();
        if (authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return authorization["Bearer ".Length..].Trim();
        }

        return null;
    }

    private static object DecodeJwtParts(string token)
    {
        var pieces = token.Split('.');
        if (pieces.Length != 3)
        {
            return new { error = "JWT 3 parcadan olusmuyor." };
        }

        return new
        {
            header = new
            {
                encoded = pieces[0],
                json = PrettyJson(Base64UrlEncoder.DecodeBytes(pieces[0]))
            },
            payload = new
            {
                encoded = pieces[1],
                json = PrettyJson(Base64UrlEncoder.DecodeBytes(pieces[1]))
            },
            signature = new
            {
                encoded = pieces[2],
                note = "HS256 imzasi. Secret key bilinmeden yeniden uretilemez."
            }
        };
    }

    private static string PrettyJson(byte[] bytes)
    {
        using var document = JsonDocument.Parse(bytes);
        return JsonSerializer.Serialize(document.RootElement, new JsonSerializerOptions { WriteIndented = true });
    }

    private static (string Salt, string Hash) SplitStoredPassword(string storedPasswordHash)
    {
        var parts = storedPasswordHash.Split(':', 2);
        return parts.Length == 2 ? (parts[0], parts[1]) : ("", storedPasswordHash);
    }

    private static string ComputePasswordHashWithStoredSalt(string password, string salt)
    {
        using var hmac = new System.Security.Cryptography.HMACSHA512(Convert.FromBase64String(salt));
        var bytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }

    public sealed class DemoLoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public sealed class VerifyTokenRequest
    {
        public string Token { get; set; } = string.Empty;
    }
}
