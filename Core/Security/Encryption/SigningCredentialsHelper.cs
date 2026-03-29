using Microsoft.IdentityModel.Tokens;

namespace Core.Security.Encryption
{
    public class SigningCredentialsHelper
    {
        public static SigningCredentials CreateSigningCredentials(SecurityKey key)
            => new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
    }
}
