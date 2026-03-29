using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Core.Security.Encryption
{
    public class SecurityKeyHelper
    {
        public static SecurityKey CreateSecurityKey(string key)
            => new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
    }
}
