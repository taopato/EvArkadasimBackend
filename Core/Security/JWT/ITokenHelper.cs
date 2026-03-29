using System.Security.Claims;
using System.Text;
using Domain.Entities;

namespace Core.Security.JWT
{
    public interface ITokenHelper
    {
        AccessToken CreateToken(User user);
    }
}
