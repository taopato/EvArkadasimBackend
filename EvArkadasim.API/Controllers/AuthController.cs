// EvArkadasim.API/Controllers/AuthController.cs
using Application.Features.Auths.Commands.AppleLogin;
using Application.Features.Auths.Commands.GoogleLogin;
using Application.Features.Auths.Commands.Login;
using Application.Features.Auths.Commands.ResetPassword;
using Application.Features.Auths.Commands.SendVerificationCode;
using Application.Features.Auths.Commands.VerifyCodeAndRegister;
using Application.Features.Auths.Dtos;
using Google.Apis.Auth;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Text.Json;

namespace EvArkadasim.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IConfiguration _configuration;
        private static readonly HttpClient _httpClient = new HttpClient();

        public AuthController(IMediator mediator, IConfiguration configuration)
        {
            _mediator = mediator;
            _configuration = configuration;
        }

        // A) Sadece kod doğrulama (reset akışı için)
        [HttpPost("VerifyCodeForReset")]
        public async Task<IActionResult> VerifyCodeForReset(
            [FromBody] VerifyCodeRequestDto dto)
        {
            var cmd = new VerifyCodeAndRegisterCommand
            {
                Email = dto.Email?.Trim() ?? string.Empty,
                Code = dto.Code?.Trim() ?? string.Empty,
                FullName = null!,     // handler’da fullname==null ise bu kod akışına girer
                Password = string.Empty
            };
            var res = await _mediator.Send(cmd);
            return Ok(res);
        }

        // B) Kayıt veya kodla kayıt akışı
        [HttpPost("VerifyCodeAndRegister")]
        public async Task<IActionResult> VerifyCodeAndRegister(
            [FromBody] VerifyCodeAndRegisterRequestDto dto)
        {
            var cmd = new VerifyCodeAndRegisterCommand
            {
                Email = dto.Email?.Trim() ?? string.Empty,
                Code = dto.Code?.Trim() ?? string.Empty,
                FullName = dto.FullName?.Trim(),
                Password = dto.Password,
                InvitationToken = dto.InvitationToken?.Trim()
            };
            var res = await _mediator.Send(cmd);
            return Ok(res);
        }

        // C) Şifre sıfırlama (yeni şifreyi set eder)
        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword(
            [FromBody] ResetPasswordRequestDto dto)
        {
            var cmd = new ResetPasswordCommand
            {
                Email = dto.Email?.Trim() ?? string.Empty,
                Code = dto.Code?.Trim() ?? string.Empty,
                NewPassword = dto.NewPassword
            };
            var res = await _mediator.Send(cmd);
            return Ok(res);
        }

        [HttpPost("SendVerificationCode")]
        public async Task<IActionResult> SendVerificationCode(
    [FromBody] SendVerificationCodeRequestDto dto)
        {
            var res = await _mediator.Send(
                new SendVerificationCodeCommand
                {
                    Email = dto.Email?.Trim() ?? string.Empty,
                    Purpose = dto.Purpose?.Trim().ToLowerInvariant() ?? "register"
                });
            return Ok(res);
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
        {
            try
            {
                var result = await _mediator.Send(new LoginCommand
                {
                    Email = dto.Email?.Trim() ?? string.Empty,
                    Password = dto.Password
                });
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpPost("GoogleLogin")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.IdToken))
                return BadRequest("IdToken zorunlu.");

            var clientIds = _configuration
                .GetSection("GoogleAuth:ClientIds")
                .Get<string[]>() ?? Array.Empty<string>();

            if (clientIds.Length == 0)
                return StatusCode(500, "Google ClientId tanımlı değil.");

            GoogleJsonWebSignature.Payload payload;
            try
            {
                payload = await GoogleJsonWebSignature.ValidateAsync(
                    dto.IdToken,
                    new GoogleJsonWebSignature.ValidationSettings
                    {
                        Audience = clientIds
                    });
            }
            catch
            {
                return Unauthorized("Geçersiz Google token.");
            }

            if (string.IsNullOrWhiteSpace(payload.Email))
                return Unauthorized("Google e-posta bilgisi alınamadı.");

            var fullName = payload.Name;
            if (string.IsNullOrWhiteSpace(fullName))
            {
                var combined = $"{payload.GivenName} {payload.FamilyName}".Trim();
                fullName = string.IsNullOrWhiteSpace(combined) ? payload.Email : combined;
            }

            var result = await _mediator.Send(new GoogleLoginCommand
            {
                Email = payload.Email,
                FullName = fullName ?? payload.Email
            });

            return Ok(result);
        }

        [HttpPost("AppleLogin")]
        public async Task<IActionResult> AppleLogin([FromBody] AppleLoginRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.IdentityToken))
                return BadRequest("IdentityToken zorunlu.");

            var bundleId = _configuration["AppleAuth:BundleId"];
            if (string.IsNullOrWhiteSpace(bundleId))
                return StatusCode(500, "Apple BundleId tanımlı değil.");

            JwtSecurityToken jwtToken;
            try
            {
                var handler = new JwtSecurityTokenHandler();
                jwtToken = handler.ReadJwtToken(dto.IdentityToken);

                var keysJson = await _httpClient.GetStringAsync("https://appleid.apple.com/auth/keys");
                var jwks = new JsonWebKeySet(keysJson);
                var signingKey = jwks.Keys.FirstOrDefault(k => k.Kid == jwtToken.Header.Kid);
                if (signingKey == null)
                    return Unauthorized("Apple imza anahtarı bulunamadı.");

                var validationParameters = new TokenValidationParameters
                {
                    ValidIssuer = "https://appleid.apple.com",
                    ValidAudience = bundleId,
                    IssuerSigningKey = signingKey,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                };

                handler.ValidateToken(dto.IdentityToken, validationParameters, out _);
            }
            catch
            {
                return Unauthorized("Geçersiz Apple token.");
            }

            var email = jwtToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
            if (string.IsNullOrWhiteSpace(email))
                return Unauthorized("Apple e-posta bilgisi alınamadı.");

            var result = await _mediator.Send(new AppleLoginCommand
            {
                Email = email,
                FullName = dto.FullName ?? email
            });

            return Ok(result);
        }
    }
}
