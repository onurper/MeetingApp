using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MeetingApp.Web.Helpers
{
    public static class JwtTokenHelper
    {
        public static int GetUserId(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);

            return int.Parse(jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value);
        }
    }
}