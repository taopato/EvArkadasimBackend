using Core.Security.Encryption;
using Core.Security.JWT;             // Core’daki ITokenHelper ve AccessToken
using Domain.Entities;               // User modeli
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System;
using System.Collections.Generic;



namespace Persistence.Security.JWT
{
    public class JwtHelper : ITokenHelper
    {
        private readonly TokenOptions _tokenOptions;

        public JwtHelper(IConfiguration configuration)
        {
            _tokenOptions = configuration
                .GetSection("TokenOptions")
                .Get<TokenOptions>();              // Binder paketi yüklü olmalı
        }

        public AccessToken CreateToken(User user)
        {
            var securityKey = SecurityKeyHelper.CreateSecurityKey(_tokenOptions.SecurityKey);
            var signingCreds = SigningCredentialsHelper.CreateSigningCredentials(securityKey);

            var jwt = new JwtSecurityToken(
                issuer: _tokenOptions.Issuer,
                audience: _tokenOptions.Audience,
                expires: DateTime.Now.AddMinutes(_tokenOptions.AccessTokenExpiration),
                notBefore: DateTime.Now,
                claims: new[]
                {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                // new Claim(ClaimTypes.Name, user.FullName ?? user.Email)
                new Claim(ClaimTypes.Name, user.Email)
                },
                signingCredentials: signingCreds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(jwt);
            return new AccessToken { Token = tokenString, Expiration = jwt.ValidTo };
        }
    }

}
