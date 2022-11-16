using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace TryTwo.Services
{
    public interface ITokenService
    {
        string GenerateAccessToken(IEnumerable<Claim> claims);
        string GenerateRefreshToken();
        public string GenerateAccessToken(string userName);
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }
}
